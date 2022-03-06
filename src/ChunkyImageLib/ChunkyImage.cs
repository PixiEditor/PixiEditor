using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ChunkyImageLibTest")]
namespace ChunkyImageLib
{
    public class ChunkyImage
    {
        private bool locked = false; //todo implement locking

        private Queue<(IOperation, HashSet<(int, int)>)> queuedOperations = new();

        private Dictionary<(int, int), Chunk> chunks = new();
        private Dictionary<(int, int), Chunk> uncommitedChunks = new();

        public Chunk? GetChunk(int x, int y)
        {
            if (queuedOperations.Count == 0)
                return MaybeGetChunk(x, y, chunks);
            ProcessQueue(x, y);
            return MaybeGetChunk(x, y, uncommitedChunks) ?? MaybeGetChunk(x, y, chunks);
        }

        private Chunk? MaybeGetChunk(int x, int y, Dictionary<(int, int), Chunk> from) => from.ContainsKey((x, y)) ? from[(x, y)] : null;

        public void DrawRectangle(ShapeData rect)
        {
            RectangleOperation operation = new(rect);
            queuedOperations.Enqueue((operation, operation.FindAffectedChunks()));
        }

        internal void DrawImage(int x, int y, Surface image)
        {
            ImageOperation operation = new(x, y, image);
            queuedOperations.Enqueue((operation, operation.FindAffectedChunks()));
        }

        public void CancelChanges()
        {
            foreach (var operation in queuedOperations)
                operation.Item1.Dispose();
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

        public HashSet<(int, int)> FindAffectedChunks()
        {
            return uncommitedChunks.Select(chunk => chunk.Key).ToHashSet();
        }

        private void ProcessQueueFinal()
        {
            foreach (var (operation, operChunks) in queuedOperations)
            {
                foreach (var (x, y) in operChunks)
                {
                    operation.DrawOnChunk(GetOrCreateCommitedChunk(x, y), x, y);
                }
                operation.Dispose();
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
            Chunk? targetChunk = null;
            foreach (var (operation, operChunks) in queuedOperations)
            {
                if (!operChunks.Contains((chunkX, chunkY)))
                    continue;
                operChunks.Remove((chunkX, chunkY));

                if (targetChunk == null)
                    targetChunk = GetOrCreateUncommitedChunk(chunkX, chunkY);

                operation.DrawOnChunk(targetChunk, chunkX, chunkY);
            }
        }

        private Chunk GetOrCreateCommitedChunk(int chunkX, int chunkY)
        {
            Chunk? targetChunk = MaybeGetChunk(chunkX, chunkY, chunks);
            if (targetChunk != null)
                return targetChunk;
            var newChunk = ChunkPool.Instance.BorrowChunk();
            newChunk.Surface.SkiaSurface.Canvas.Clear();
            chunks.Add((chunkX, chunkY), newChunk);
            return newChunk;
        }

        private Chunk GetOrCreateUncommitedChunk(int chunkX, int chunkY)
        {
            Chunk? targetChunk;
            targetChunk = MaybeGetChunk(chunkX, chunkY, uncommitedChunks);
            if (targetChunk == null)
            {
                targetChunk = ChunkPool.Instance.BorrowChunk();
                var maybeCommitedChunk = MaybeGetChunk(chunkX, chunkY, chunks);
                if (maybeCommitedChunk != null)
                    maybeCommitedChunk.Surface.CopyTo(targetChunk.Surface);
                else
                    targetChunk.Surface.SkiaSurface.Canvas.Clear();
            }
            uncommitedChunks.Add((chunkX, chunkY), targetChunk);
            return targetChunk;
        }
    }
}
