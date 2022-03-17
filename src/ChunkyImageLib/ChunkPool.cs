using ChunkyImageLib.DataHolders;

namespace ChunkyImageLib
{
    internal class ChunkPool
    {
        public const int ChunkSize = 32;
        public static Vector2i ChunkSizeVec => new(ChunkSize, ChunkSize);
        // not thread-safe!
        private static ChunkPool? instance;
        public static ChunkPool Instance => instance ??= new ChunkPool();

        private List<Chunk> freeChunks = new();
        private HashSet<Chunk> usedChunks = new();

        public Chunk TransparentChunk { get; } = new Chunk();

        public Chunk BorrowChunk(object borrowee)
        {
            Chunk chunk;
            if (freeChunks.Count > 0)
            {
                chunk = freeChunks[^1];
                freeChunks.RemoveAt(freeChunks.Count - 1);
            }
            else
            {
                chunk = new Chunk();
            }
            usedChunks.Add(chunk);

            return chunk;
        }

        public void ReturnChunk(Chunk chunk)
        {
            if (!usedChunks.Contains(chunk))
            {
                if (freeChunks.Contains(chunk))
                    throw new Exception("This chunk has already been returned");
                throw new Exception("This chunk wasn't borrowed or was already returned and disposed");
            }
            usedChunks.Remove(chunk);
            freeChunks.Add(chunk);

            MaybeDisposeChunks();
        }

        private void MaybeDisposeChunks()
        {
            for (int i = freeChunks.Count - 1; i > 200; i--)
            {
                freeChunks[i].Dispose();
                freeChunks.RemoveAt(i);
            }
        }
    }
}
