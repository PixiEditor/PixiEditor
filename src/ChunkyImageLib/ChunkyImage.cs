using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using SkiaSharp;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ChunkyImageLibTest")]
namespace ChunkyImageLib
{
    public class ChunkyImage : IReadOnlyChunkyImage
    {
        private Queue<(IOperation, HashSet<Vector2i>)> queuedOperations = new();

        private Dictionary<Vector2i, Chunk> commitedChunks = new();
        private Dictionary<Vector2i, Chunk> latestChunks = new();
        private Chunk tempChunk;

        private static SKPaint ClippingPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.DstIn };

        public static int ChunkSize => ChunkPool.ChunkSize;

        public ChunkyImage()
        {
            tempChunk = ChunkPool.Instance.BorrowChunk();
        }

        public ChunkyImage CloneFromLatest()
        {
            ChunkyImage output = new();
            var chunks = FindAllChunks();
            foreach (var chunk in chunks)
            {
                var image = GetLatestChunk(chunk);
                if (image != null)
                    output.DrawImage(chunk * ChunkSize, image.Surface);
            }
            output.CommitChanges();
            return output;
        }

        public Chunk? GetLatestChunk(Vector2i pos)
        {
            if (queuedOperations.Count == 0)
                return MaybeGetChunk(pos, commitedChunks);
            ProcessQueue(pos);
            return MaybeGetChunk(pos, latestChunks) ?? MaybeGetChunk(pos, commitedChunks);
        }

        internal Chunk? GetCommitedChunk(Vector2i pos)
        {
            return MaybeGetChunk(pos, commitedChunks);
        }

        private Chunk? MaybeGetChunk(Vector2i pos, Dictionary<Vector2i, Chunk> from) => from.ContainsKey(pos) ? from[pos] : null;

        public void DrawRectangle(ShapeData rect)
        {
            RectangleOperation operation = new(rect);
            queuedOperations.Enqueue((operation, operation.FindAffectedChunks(this)));
        }

        internal void DrawImage(Vector2i pos, Surface image)
        {
            ImageOperation operation = new(pos, image);
            queuedOperations.Enqueue((operation, operation.FindAffectedChunks(this)));
        }

        public void Clear()
        {
            ClearOperation operation = new();
            queuedOperations.Enqueue((operation, operation.FindAffectedChunks(this)));
        }

        public void ApplyRasterClip(ChunkyImage clippingMask)
        {
            RasterClipOperation operation = new(clippingMask);
            queuedOperations.Enqueue((operation, new()));
        }

        public void CancelChanges()
        {
            foreach (var operation in queuedOperations)
                operation.Item1.Dispose();
            queuedOperations.Clear();
            foreach (var (_, chunk) in latestChunks)
            {
                ChunkPool.Instance.ReturnChunk(chunk);
            }
            latestChunks.Clear();
        }

        public void CommitChanges()
        {
            var affectedChunks = FindAffectedChunks();
            foreach (var chunk in affectedChunks)
            {
                ProcessQueue(chunk);
            }
            foreach (var (operation, operChunks) in queuedOperations)
                operation.Dispose();
            queuedOperations.Clear();
            CommitLatestChunks();
        }

        public HashSet<Vector2i> FindAllChunks()
        {
            var allChunks = commitedChunks.Select(chunk => chunk.Key).ToHashSet();
            allChunks.UnionWith(latestChunks.Select(chunk => chunk.Key).ToHashSet());
            foreach (var (operation, opChunks) in queuedOperations)
            {
                allChunks.UnionWith(opChunks);
            }
            return allChunks;
        }

        public HashSet<Vector2i> FindAffectedChunks()
        {
            var chunks = latestChunks.Select(chunk => chunk.Key).ToHashSet();
            foreach (var (operation, opChunks) in queuedOperations)
            {
                chunks.UnionWith(opChunks);
            }
            return chunks;
        }

        private void CommitLatestChunks()
        {
            foreach (var (pos, chunk) in latestChunks)
            {
                if (commitedChunks.ContainsKey(pos))
                {
                    var oldChunk = commitedChunks[pos];
                    commitedChunks.Remove(pos);
                    ChunkPool.Instance.ReturnChunk(oldChunk);
                }
                commitedChunks.Add(pos, chunk);
            }
            latestChunks.Clear();
        }

        private void ProcessQueue(Vector2i chunkPos)
        {
            Chunk? targetChunk = null;
            List<RasterClipOperation> clips = new();
            foreach (var (operation, operChunks) in queuedOperations)
            {
                if (operation is IChunkOperation chunkOperation)
                {
                    if (!operChunks.Contains(chunkPos))
                        continue;
                    operChunks.Remove(chunkPos);

                    if (targetChunk == null)
                        targetChunk = GetOrCreateLatestChunk(chunkPos);

                    if (clips.Count == 0)
                    {
                        chunkOperation.DrawOnChunk(targetChunk, chunkPos);
                        continue;
                    }

                    List<Chunk> masks = new();
                    foreach (var clip in clips)
                    {
                        var chunk = clip.ClippingMask.GetCommitedChunk(chunkPos);
                        if (chunk == null)
                        {
                            //the chunked is fully masked out, no point to draw any further operations
                            return;
                        }
                        masks.Add(chunk);
                    }

                    if (masks.Count == 0)
                    {
                        chunkOperation.DrawOnChunk(targetChunk, chunkPos);
                    }
                    else
                    {
                        tempChunk.Surface.SkiaSurface.Canvas.Clear();
                        chunkOperation.DrawOnChunk(tempChunk, chunkPos);
                        foreach (var mask in masks)
                        {
                            tempChunk.Surface.SkiaSurface.Canvas.DrawSurface(mask.Surface.SkiaSurface, 0, 0, ClippingPaint);
                        }
                        tempChunk.DrawOnSurface(targetChunk.Surface.SkiaSurface, new(0, 0));
                    }
                }
                else if (operation is RasterClipOperation clipOperation)
                {
                    clips.Add(clipOperation);
                }
            }
        }

        public bool CheckIfCommitedIsEmpty()
        {
            FindAndDeleteEmptyCommitedChunks();
            return commitedChunks.Count == 0;
        }

        private void FindAndDeleteEmptyCommitedChunks()
        {
            foreach (var (pos, chunk) in commitedChunks)
            {
                if (IsChunkEmpty(chunk))
                    commitedChunks.Remove(pos);
            }
        }

        private unsafe bool IsChunkEmpty(Chunk chunk)
        {
            ulong* ptr = (ulong*)chunk.Surface.PixelBuffer;
            for (int i = 0; i < ChunkSize * ChunkSize; i++)
            {
                // ptr[i] actually contains 4 16-bit floats. We only care about the first one which is alpha.
                // An empty pixel can have alpha of 0 or -0 (not sure if -0 actually ever comes up). 0 in hex is 0x0, -0 in hex is 0x8000
                if ((ptr[i] & 0x1111_0000_0000_0000) != 0 && (ptr[i] & 0x1111_0000_0000_0000) != 0x8000_0000_0000_0000)
                    return false;
            }
            return true;
        }

        private Chunk GetOrCreateCommitedChunk(Vector2i chunkPos)
        {
            Chunk? targetChunk = MaybeGetChunk(chunkPos, commitedChunks);
            if (targetChunk != null)
                return targetChunk;
            var newChunk = ChunkPool.Instance.BorrowChunk();
            newChunk.Surface.SkiaSurface.Canvas.Clear();
            commitedChunks[chunkPos] = newChunk;
            return newChunk;
        }

        private Chunk GetOrCreateLatestChunk(Vector2i chunkPos)
        {
            Chunk? targetChunk;
            targetChunk = MaybeGetChunk(chunkPos, latestChunks);
            if (targetChunk == null)
            {
                targetChunk = ChunkPool.Instance.BorrowChunk();
                var maybeCommitedChunk = MaybeGetChunk(chunkPos, commitedChunks);

                if (maybeCommitedChunk != null)
                    maybeCommitedChunk.Surface.CopyTo(targetChunk.Surface);
                else
                    targetChunk.Surface.SkiaSurface.Canvas.Clear();

                latestChunks[chunkPos] = targetChunk;
            }
            return targetChunk;
        }
    }
}
