using ChunkyImageLib.DataHolders;

namespace ChunkyImageLib
{
    public class ChunkStorage : IDisposable
    {
        private bool disposed = false;
        private List<(Vector2i, Chunk?)> savedChunks = new();
        public ChunkStorage(ChunkyImage image, HashSet<Vector2i> commitedChunksToSave)
        {
            foreach (var chunkPos in commitedChunksToSave)
            {
                Chunk? chunk = image.GetCommitedChunk(chunkPos);
                if (chunk == null)
                {
                    savedChunks.Add((chunkPos, null));
                    continue;
                }
                Chunk copy = ChunkPool.Instance.BorrowChunk();
                chunk.Surface.CopyTo(copy.Surface);
                savedChunks.Add((chunkPos, copy));
            }
        }

        public void ApplyChunksToImage(ChunkyImage image)
        {
            if (disposed)
                throw new Exception("This instance has been disposed");
            foreach (var (pos, chunk) in savedChunks)
            {
                if (chunk == null)
                    image.DrawImage(pos * ChunkPool.ChunkSize, ChunkPool.Instance.TransparentChunk.Surface);
                else
                    image.DrawImage(pos * ChunkPool.ChunkSize, chunk.Surface);
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            foreach (var (pos, chunk) in savedChunks)
            {
                if (chunk != null)
                    ChunkPool.Instance.ReturnChunk(chunk);
            }
        }
    }
}
