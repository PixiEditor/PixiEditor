namespace ChunkyImageLib.DataHolders;

[Flags]
public enum ChunkResolution
{
    /// <summary>
    /// The full resolution of the chunk
    /// </summary>
    Full = 1,
    /// <summary>
    /// Half of the chunks resolution
    /// </summary>
    Half = 2,
    /// <summary>
    /// A quarter of the chunks resolution
    /// </summary>
    Quarter = 4,
    /// <summary>
    /// An eighth of the chunks resolution
    /// </summary>
    Eighth = 8
}
