namespace ChunkyImageLib.Operations
{
    public static class OperationHelper
    {
        public static (int, int) GetChunkPos(int pixelX, int pixelY, int chunkSize)
        {
            int x = (int)Math.Floor(pixelX / (float)chunkSize);
            int y = (int)Math.Floor(pixelY / (float)chunkSize);
            return (x, y);
        }
    }
}
