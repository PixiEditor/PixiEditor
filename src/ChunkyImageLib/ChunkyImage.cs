using ChunkyImageLib.Operations;
using SkiaSharp;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ChunkyImageLibTest")]
namespace ChunkyImageLib
{
    public class ChunkyImage
    {
        private bool locked = false; //todo implement locking

        private Queue<(IOperation, HashSet<(int, int)>)> queuedOperations = new();

        private Dictionary<(int, int), ImageData> chunks = new();
        private Dictionary<(int, int), ImageData> uncommitedChunks = new();

        public ImageData? GetChunk(int x, int y)
        {
            if (queuedOperations.Count == 0)
                return MaybeGetChunk(x, y, chunks);
            ProcessQueue(x, y);
            return MaybeGetChunk(x, y, uncommitedChunks) ?? MaybeGetChunk(x, y, chunks);
        }

        private ImageData? MaybeGetChunk(int x, int y, Dictionary<(int, int), ImageData> from) => from.ContainsKey((x, y)) ? from[(x, y)] : null;

        public void DrawRectangle(int x, int y, int width, int height, int strokeThickness, SKColor strokeColor, SKColor fillColor)
        {
            RectangleOperation operation = new(x, y, width, height, strokeThickness, strokeColor, fillColor);
            queuedOperations.Enqueue((operation, operation.FindAffectedChunks(ChunkPool.ChunkSize)));
        }

        public void CancelChanges()
        {
            queuedOperations.Clear();
            foreach (var (_, chunk) in uncommitedChunks)
            {
                ChunkPool.Instance.ReturnChunk(chunk);
            }
        }

        public void CommitChanges()
        {
            SwapUncommitedChunks();
            ProcessQueueFinal();
        }

        private void ProcessQueueFinal()
        {
            foreach (var (operation, operChunks) in queuedOperations)
            {
                foreach (var (x, y) in operChunks)
                {
                    operation.DrawOnChunk(GetOrCreateCommitedChunk(x, y), x, y);
                }
            }
            queuedOperations.Clear();
        }

        private void SwapUncommitedChunks()
        {
            foreach (var (pos, chunk) in uncommitedChunks)
            {
                if (chunks.ContainsKey(pos))
                {
                    var oldChunk = chunks[pos];
                    chunks.Remove(pos);
                    ChunkPool.Instance.ReturnChunk(oldChunk);
                }
                chunks.Add(pos, chunk);
            }
        }

        private void ProcessQueue(int chunkX, int chunkY)
        {
            ImageData? targetChunk = null;
            foreach (var (operation, operChunks) in queuedOperations)
            {
                if (!operChunks.Contains((chunkX, chunkY)))
                    continue;
                operChunks.Remove((chunkX, chunkY));

                if (targetChunk == null)
                    targetChunk = GetOrCreateUncommitedChunk(chunkX, chunkY);

                operation.DrawOnChunk(targetChunk, chunkX, chunkY);
            }
            queuedOperations.Clear();
        }

        private ImageData GetOrCreateCommitedChunk(int chunkX, int chunkY)
        {
            ImageData? targetChunk = MaybeGetChunk(chunkX, chunkY, chunks);
            if (targetChunk != null)
                return targetChunk;
            var newChunk = ChunkPool.Instance.BorrowChunk();
            newChunk.SkiaSurface.Canvas.Clear();
            chunks.Add((chunkX, chunkY), newChunk);
            return newChunk;
        }

        private ImageData GetOrCreateUncommitedChunk(int chunkX, int chunkY)
        {
            ImageData? targetChunk;
            targetChunk = MaybeGetChunk(chunkX, chunkY, uncommitedChunks);
            if (targetChunk == null)
            {
                targetChunk = ChunkPool.Instance.BorrowChunk();
                var maybeCommitedChunk = MaybeGetChunk(chunkX, chunkY, chunks);
                if (maybeCommitedChunk != null)
                    maybeCommitedChunk.CopyTo(targetChunk);
                else
                    targetChunk.SkiaSurface.Canvas.Clear();
            }
            uncommitedChunks.Add((chunkX, chunkY), targetChunk);
            return targetChunk;
        }
    }
}
