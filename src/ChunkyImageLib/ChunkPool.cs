namespace ChunkyImageLib
{
    internal class ChunkPool
    {
        public const int ChunkSize = 32;

        // not thread-safe!
        private static ChunkPool? instance;
        public static ChunkPool Instance => instance ??= new ChunkPool();

        private List<Chunk> freeChunks = new();
        private HashSet<Chunk> usedChunks = new();

        public Chunk TransparentChunk { get; } = new Chunk();

        public Chunk BorrowChunk()
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
                throw new Exception("This chunk wasn't borrowed");
            usedChunks.Remove(chunk);
            freeChunks.Add(chunk);

            MaybeDisposeChunks();
        }

        private void MaybeDisposeChunks()
        {
            for (int i = freeChunks.Count - 1; i >= 100; i++)
            {
                freeChunks[i].Dispose();
                freeChunks.RemoveAt(i);
            }
        }
    }
}
