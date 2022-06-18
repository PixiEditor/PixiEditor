namespace ChunkyImageLib.DataHolders;

public static class ChunkResolutionEx
{
    /// <summary>
    /// Returns the multiplier of the <paramref name="resolution"/>.
    /// </summary>
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

    /// <summary>
    /// Returns the <see cref="ChunkPool.FullChunkSize"/> for the <paramref name="resolution"/>
    /// </summary>
    /// <seealso cref="ChunkPool"/>
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
