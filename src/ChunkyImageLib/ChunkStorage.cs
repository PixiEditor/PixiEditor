namespace ChunkyImageLib
{
    public class ChunkStorage : IDisposable
    {
        private bool disposed = false;
        private List<(int, int, Chunk?)> savedChunks = new();
        public ChunkStorage(ChunkyImage image, HashSet<(int, int)> chunksToSave)
        {
            foreach (var (x, y) in chunksToSave)
            {
                Chunk? chunk = image.GetChunk(x, y);
                if (chunk == null)
                {
                    savedChunks.Add((x, y, null));
                    continue;
                }
                Chunk copy = ChunkPool.Instance.BorrowChunk();
                chunk.Surface.CopyTo(copy.Surface);
                savedChunks.Add((x, y, copy));
            }
        }

        public void ApplyChunksToImage(ChunkyImage image)
        {
            if (disposed)
                throw new Exception("This instance has been disposed");
            foreach (var (x, y, chunk) in savedChunks)
            {
                if (chunk == null)
                    image.DrawImage(x * ChunkPool.ChunkSize, y * ChunkPool.ChunkSize, ChunkPool.Instance.TransparentChunk.Surface);
                else
                    image.DrawImage(x * ChunkPool.ChunkSize, y * ChunkPool.ChunkSize, chunk.Surface);
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            foreach (var (x, y, chunk) in savedChunks)
            {
                if (chunk != null)
                    ChunkPool.Instance.ReturnChunk(chunk);
            }
        }
    }
}
