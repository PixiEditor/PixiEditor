using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using SkiaSharp;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ChunkyImageLibTest")]
namespace ChunkyImageLib
{
    public class ChunkyImage : IReadOnlyChunkyImage, IDisposable
    {
        private struct LatestChunkData
        {
            public int QueueProgress { get; set; } = 0;
            public bool IsDeleted { get; set; } = false;
        }
        private bool disposed = false;

        public static int ChunkSize => ChunkPool.FullChunkSize;
        private static SKPaint ClippingPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.DstIn };
        private Chunk tempChunk;

        public Vector2i CommitedSize { get; private set; }
        public Vector2i LatestSize { get; private set; }

        private List<(IOperation operation, HashSet<Vector2i> affectedChunks)> queuedOperations = new();

        private Dictionary<Vector2i, Chunk> commitedChunks = new();
        private Dictionary<Vector2i, Chunk> latestChunks = new();
        private Dictionary<Vector2i, LatestChunkData> latestChunksData = new();

        public ChunkyImage(Vector2i size)
        {
            CommitedSize = size;
            LatestSize = size;
            tempChunk = Chunk.Create();
        }

        public ChunkyImage CloneFromLatest()
        {
            ChunkyImage output = new(LatestSize);
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

        /// <summary>
        /// Returns the latest version of the chunk, with uncommited changes applied if they exist
        /// </summary>
        public Chunk? GetLatestChunk(Vector2i pos)
        {
            if (queuedOperations.Count == 0)
                return MaybeGetChunk(pos, commitedChunks);
            ProcessQueueForChunk(pos);
            return MaybeGetChunk(pos, latestChunks) ?? MaybeGetChunk(pos, commitedChunks);
        }

        /// <summary>
        /// Returns the commited version of the chunk ignoring any uncommited changes
        /// </summary>
        internal Chunk? GetCommitedChunk(Vector2i pos)
        {
            return MaybeGetChunk(pos, commitedChunks);
        }

        private Chunk? MaybeGetChunk(Vector2i pos, Dictionary<Vector2i, Chunk> from) => from.ContainsKey(pos) ? from[pos] : null;

        public void DrawRectangle(ShapeData rect)
        {
            RectangleOperation operation = new(rect);
            EnqueueOperation(operation);
        }

        internal void DrawImage(Vector2i pos, Surface image)
        {
            ImageOperation operation = new(pos, image);
            EnqueueOperation(operation);
        }

        internal void ClearRegion(Vector2i pos, Vector2i size)
        {
            ClearRegionOperation operation = new(pos, size);
            EnqueueOperation(operation);
        }

        public void Clear()
        {
            ClearOperation operation = new();
            EnqueueOperation(operation, FindAllChunks());
        }

        public void ApplyRasterClip(ChunkyImage clippingMask)
        {
            RasterClipOperation operation = new(clippingMask);
            EnqueueOperation(operation, new());
        }

        public void Resize(Vector2i newSize)
        {
            ResizeOperation operation = new(newSize);
            LatestSize = newSize;
            EnqueueOperation(operation, FindAllChunksOutsideBounds(newSize));
        }

        private void EnqueueOperation(IDrawOperation operation)
        {
            var chunks = operation.FindAffectedChunks();
            chunks.RemoveWhere(pos => IsOutsideBounds(pos, LatestSize));
            if (operation.IgnoreEmptyChunks)
                chunks.IntersectWith(FindAllChunks());
            EnqueueOperation(operation, chunks);
        }
        private void EnqueueOperation(IOperation operation, HashSet<Vector2i> chunks)
        {
            queuedOperations.Add((operation, chunks));
        }

        public void CancelChanges()
        {
            foreach (var operation in queuedOperations)
                operation.Item1.Dispose();
            queuedOperations.Clear();
            foreach (var (_, chunk) in latestChunks)
            {
                chunk.Dispose();
            }
            LatestSize = CommitedSize;
            latestChunks.Clear();
            latestChunksData.Clear();
        }

        public void CommitChanges()
        {
            var affectedChunks = FindAffectedChunks();
            foreach (var chunk in affectedChunks)
            {
                ProcessQueueForChunk(chunk);
            }
            foreach (var (operation, _) in queuedOperations)
            {
                operation.Dispose();
            }
            CommitLatestChunks();
            CommitedSize = LatestSize;
            queuedOperations.Clear();
        }

        /// <summary>
        /// Returns all chunks that have something in them, including latest (uncommited) ones
        /// </summary>
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

        /// <summary>
        /// Returns chunks affected by operations that haven't been commited yet
        /// </summary>
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
                LatestChunkData data = latestChunksData[pos];
                if (data.QueueProgress != queuedOperations.Count)
                    throw new Exception("Trying to commit chunk that wasn't fully processed");

                if (commitedChunks.ContainsKey(pos))
                {
                    var oldChunk = commitedChunks[pos];
                    commitedChunks.Remove(pos);
                    oldChunk.Dispose();
                }
                if (!data.IsDeleted)
                    commitedChunks.Add(pos, chunk);
                else
                    chunk.Dispose();
            }

            latestChunks.Clear();
            latestChunksData.Clear();
        }

        private void ProcessQueueForChunk(Vector2i chunkPos)
        {
            Chunk? targetChunk = null;
            if (latestChunksData.TryGetValue(chunkPos, out LatestChunkData chunkData))
                chunkData = new() { QueueProgress = 0, IsDeleted = !commitedChunks.ContainsKey(chunkPos) };

            if (chunkData.QueueProgress == queuedOperations.Count)
                return;

            List<Chunk> activeClips = new();
            bool isFullyMaskedOut = false;
            bool somethingWasApplied = false;
            for (int i = 0; i < queuedOperations.Count; i++)
            {
                var (operation, operChunks) = queuedOperations[i];
                if (operation is RasterClipOperation clipOperation)
                {
                    var chunk = clipOperation.ClippingMask.GetCommitedChunk(chunkPos);
                    if (chunk != null)
                        activeClips.Add(chunk);
                    else
                        isFullyMaskedOut = true;
                }

                if (!operChunks.Contains(chunkPos))
                    continue;
                if (!somethingWasApplied)
                {
                    somethingWasApplied = true;
                    targetChunk = GetOrCreateLatestChunk(chunkPos);
                }

                if (chunkData.QueueProgress <= i)
                    chunkData.IsDeleted = ApplyOperationToChunk(operation, activeClips, isFullyMaskedOut, targetChunk!, chunkPos, chunkData);
            }

            if (somethingWasApplied)
            {
                chunkData.QueueProgress = queuedOperations.Count;
                latestChunksData[chunkPos] = chunkData;
            }
        }

        private bool ApplyOperationToChunk(
            IOperation operation,
            List<Chunk> activeClips,
            bool isFullyMaskedOut,
            Chunk targetChunk,
            Vector2i chunkPos,
            LatestChunkData chunkData)
        {
            if (operation is ClearOperation)
                return true;

            if (operation is IDrawOperation chunkOperation)
            {
                if (isFullyMaskedOut)
                    return chunkData.IsDeleted;

                if (chunkData.IsDeleted)
                    targetChunk.Surface.SkiaSurface.Canvas.Clear();
                if (activeClips.Count == 0)
                {
                    chunkOperation.DrawOnChunk(targetChunk, chunkPos);
                    return false;
                }

                tempChunk.Surface.SkiaSurface.Canvas.Clear();
                chunkOperation.DrawOnChunk(tempChunk, chunkPos);
                foreach (var mask in activeClips)
                {
                    tempChunk.Surface.SkiaSurface.Canvas.DrawSurface(mask.Surface.SkiaSurface, 0, 0, ClippingPaint);
                }
                tempChunk.DrawOnSurface(targetChunk.Surface.SkiaSurface, new(0, 0));
                return false;
            }

            if (operation is ResizeOperation resizeOperation)
            {
                return IsOutsideBounds(chunkPos, resizeOperation.Size);
            }
            return chunkData.IsDeleted;
        }

        public bool CheckIfCommitedIsEmpty()
        {
            FindAndDeleteEmptyCommitedChunks();
            return commitedChunks.Count == 0;
        }

        private HashSet<Vector2i> FindAllChunksOutsideBounds(Vector2i size)
        {
            var chunks = FindAllChunks();
            chunks.RemoveWhere(pos => !IsOutsideBounds(pos, size));
            return chunks;
        }

        private static bool IsOutsideBounds(Vector2i chunkPos, Vector2i imageSize)
        {
            return chunkPos.X < 0 || chunkPos.Y < 0 || chunkPos.X * ChunkSize >= imageSize.X || chunkPos.Y * ChunkSize >= imageSize.Y;
        }

        private void FindAndDeleteEmptyCommitedChunks()
        {
            if (queuedOperations.Count != 0)
                throw new Exception("This method cannot be used while any operations are queued");
            HashSet<Vector2i> toRemove = new();
            foreach (var (pos, chunk) in commitedChunks)
            {
                if (IsChunkEmpty(chunk))
                {
                    toRemove.Add(pos);
                    chunk.Dispose();
                }
            }
            foreach (var pos in toRemove)
                commitedChunks.Remove(pos);
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

        private Chunk GetOrCreateLatestChunk(Vector2i chunkPos)
        {
            Chunk? targetChunk;
            targetChunk = MaybeGetChunk(chunkPos, latestChunks);
            if (targetChunk == null)
            {
                targetChunk = Chunk.Create();
                var maybeCommitedChunk = MaybeGetChunk(chunkPos, commitedChunks);

                if (maybeCommitedChunk != null)
                    maybeCommitedChunk.Surface.CopyTo(targetChunk.Surface);
                else
                    targetChunk.Surface.SkiaSurface.Canvas.Clear();

                latestChunks[chunkPos] = targetChunk;
            }
            return targetChunk;
        }

        public void Dispose()
        {
            if (disposed)
                return;
            CancelChanges();
            tempChunk.Dispose();
            foreach (var chunk in commitedChunks)
                chunk.Value.Dispose();
            disposed = true;
        }
    }
}
