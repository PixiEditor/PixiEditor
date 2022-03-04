namespace ChunkyImageLib
{
    internal class ChunkPool
    {
        // not thread-safe!
        public const int ChunkSize = 32;

        private static ChunkPool? instance;
        public static ChunkPool Instance => instance ??= new ChunkPool();

        private List<ImageData> freeChunks = new();
        private HashSet<ImageData> usedChunks = new();

        public ImageData BorrowChunk()
        {
            ImageData chunk;
            if (freeChunks.Count > 0)
            {
                chunk = freeChunks[^1];
                freeChunks.RemoveAt(freeChunks.Count - 1);
            }
            else
            {
                chunk = new ImageData(ChunkSize, ChunkSize, SkiaSharp.SKColorType.RgbaF16);
            }
            usedChunks.Add(chunk);

            return chunk;
        }

        public void ReturnChunk(ImageData chunk)
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
