using ChunkyImageLib.DataHolders;

namespace ChunkyImageLib
{
    public class CommittedChunkStorage : IDisposable
    {
        private bool disposed = false;
        private List<(Vector2i, Chunk?)> savedChunks = new();
        public CommittedChunkStorage(ChunkyImage image, HashSet<Vector2i> committedChunksToSave)
        {
            foreach (var chunkPos in committedChunksToSave)
            {
                Chunk? chunk = (Chunk?)image.GetCommittedChunk(chunkPos, ChunkResolution.Full);
                if (chunk is null)
                {
                    savedChunks.Add((chunkPos, null));
                    continue;
                }
                Chunk copy = Chunk.Create();
                chunk.Surface.CopyTo(copy.Surface);
                savedChunks.Add((chunkPos, copy));
            }
        }

        public void ApplyChunksToImage(ChunkyImage image)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(CommittedChunkStorage));
            foreach (var (pos, chunk) in savedChunks)
            {
                if (chunk is null)
                    image.ClearRegion(pos * ChunkPool.FullChunkSize, new(ChunkPool.FullChunkSize, ChunkPool.FullChunkSize));
                else
                    image.DrawImage(pos * ChunkPool.FullChunkSize, chunk.Surface);
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            foreach (var (_, chunk) in savedChunks)
            {
                if (chunk is not null)
                    chunk.Dispose();
            }
        }
    }
}
