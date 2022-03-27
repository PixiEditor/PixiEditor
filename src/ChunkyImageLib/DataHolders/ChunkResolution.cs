namespace ChunkyImageLib.DataHolders
{
    public enum ChunkResolution
    {
        Full,
        Half,
        Quarter,
        Eighth
    }

    public static class ChunkResolutionEx
    {
        public static double Multiplier(this ChunkResolution resolution)
        {
            return resolution switch
            {
                ChunkResolution.Full => 1.0,
                ChunkResolution.Half => 1.0 / 2,
                ChunkResolution.Quarter => 1.0 / 4,
                ChunkResolution.Eighth => 1.0 / 8,
                _ => 1,
            };
        }

        public static int PixelSize(this ChunkResolution resolution)
        {
            return resolution switch
            {
                ChunkResolution.Full => ChunkPool.FullChunkSize,
                ChunkResolution.Half => ChunkPool.FullChunkSize / 2,
                ChunkResolution.Quarter => ChunkPool.FullChunkSize / 4,
                ChunkResolution.Eighth => ChunkPool.FullChunkSize / 8,
                _ => ChunkPool.FullChunkSize
            };
        }
    }
}
