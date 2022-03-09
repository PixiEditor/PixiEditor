using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ChunkyImageLibTest")]
namespace ChunkyImageLib
{
    public class ChunkyImage
    {
        private bool locked = false; //todo implement locking

        private Queue<(IOperation, HashSet<Vector2i>)> queuedOperations = new();

        private Dictionary<Vector2i, Chunk> chunks = new();
        private Dictionary<Vector2i, Chunk> uncommitedChunks = new();

        public static int ChunkSize => ChunkPool.ChunkSize;

        public Chunk? GetChunk(Vector2i pos)
        {
            if (queuedOperations.Count == 0)
                return MaybeGetChunk(pos, chunks);
            ProcessQueue(pos);
            return MaybeGetChunk(pos, uncommitedChunks) ?? MaybeGetChunk(pos, chunks);
        }

        public Chunk? GetCommitedChunk(Vector2i pos)
        {
            return MaybeGetChunk(pos, chunks);
        }

        private Chunk? MaybeGetChunk(Vector2i pos, Dictionary<Vector2i, Chunk> from) => from.ContainsKey(pos) ? from[pos] : null;

        public void DrawRectangle(ShapeData rect)
        {
            RectangleOperation operation = new(rect);
            queuedOperations.Enqueue((operation, operation.FindAffectedChunks()));
        }

        internal void DrawImage(Vector2i pos, Surface image)
        {
            ImageOperation operation = new(pos, image);
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
            uncommitedChunks.Clear();
        }

        public void CommitChanges()
        {
            SwapUncommitedChunks();
            ProcessQueueFinal();
        }

        public HashSet<Vector2i> FindAffectedChunks()
        {
            var chunks = uncommitedChunks.Select(chunk => chunk.Key).ToHashSet();
            foreach (var (operation, opChunks) in queuedOperations)
            {
                chunks.UnionWith(opChunks);
            }
            return chunks;
        }

        private void ProcessQueueFinal()
        {
            foreach (var (operation, operChunks) in queuedOperations)
            {
                foreach (var pos in operChunks)
                {
                    operation.DrawOnChunk(GetOrCreateCommitedChunk(pos), pos);
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
            uncommitedChunks.Clear();
        }

        private void ProcessQueue(Vector2i chunkPos)
        {
            Chunk? targetChunk = null;
            foreach (var (operation, operChunks) in queuedOperations)
            {
                if (!operChunks.Contains(chunkPos))
                    continue;
                operChunks.Remove(chunkPos);

                if (targetChunk == null)
                    targetChunk = GetOrCreateUncommitedChunk(chunkPos);

                operation.DrawOnChunk(targetChunk, chunkPos);
            }
        }

        private Chunk GetOrCreateCommitedChunk(Vector2i chunkPos)
        {
            Chunk? targetChunk = MaybeGetChunk(chunkPos, chunks);
            if (targetChunk != null)
                return targetChunk;
            var newChunk = ChunkPool.Instance.BorrowChunk();
            newChunk.Surface.SkiaSurface.Canvas.Clear();
            chunks[chunkPos] = newChunk;
            return newChunk;
        }

        private Chunk GetOrCreateUncommitedChunk(Vector2i chunkPos)
        {
            Chunk? targetChunk;
            targetChunk = MaybeGetChunk(chunkPos, uncommitedChunks);
            if (targetChunk == null)
            {
                targetChunk = ChunkPool.Instance.BorrowChunk();
                var maybeCommitedChunk = MaybeGetChunk(chunkPos, chunks);

                if (maybeCommitedChunk != null)
                    maybeCommitedChunk.Surface.CopyTo(targetChunk.Surface);
                else
                    targetChunk.Surface.SkiaSurface.Canvas.Clear();

                uncommitedChunks[chunkPos] = targetChunk;
            }
            return targetChunk;
        }
    }
}
