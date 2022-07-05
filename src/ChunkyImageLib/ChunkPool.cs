using ChunkyImageLib.DataHolders;
using System.Collections.Concurrent;

namespace ChunkyImageLib;

internal class ChunkPool
{
    //must be divisible by 8
    public const int FullChunkSize = 256;

    private static object lockObj = new();
    private static ChunkPool? instance;
    /// <summary>
    /// The instance of the <see cref="ChunkPool"/>
    /// </summary>
    public static ChunkPool Instance
    {
        get
        {
            if (instance is null)
            {
                lock (lockObj)
                {
                    instance ??= new ChunkPool();
                }
            }
            return instance;
        }
    }

    private readonly ConcurrentBag<Chunk> fullChunks = new();
    private readonly ConcurrentBag<Chunk> halfChunks = new();
    private readonly ConcurrentBag<Chunk> quarterChunks = new();
    private readonly ConcurrentBag<Chunk> eighthChunks = new();
    
    /// <summary>
    /// Tries to take a chunk from the pool, returns null if there's no Chunk available
    /// </summary>
    /// <param name="resolution">The resolution for the chunk</param>
    internal Chunk? Get(ChunkResolution resolution) => GetBag(resolution).TryTake(out Chunk? item) ? item : null;

    private ConcurrentBag<Chunk> GetBag(ChunkResolution resolution)
    {
        return resolution switch
        {
            ChunkResolution.Full => fullChunks,
            ChunkResolution.Half => halfChunks,
            ChunkResolution.Quarter => quarterChunks,
            ChunkResolution.Eighth => eighthChunks,
            _ => fullChunks
        };
    }

    /// <summary>
    /// Returns a chunk back to the pool
    /// </summary>
    internal void Push(Chunk chunk)
    {
        var chunks = GetBag(chunk.Resolution);
        //a race condition can cause the count to go above 200, but likely not by much
        if (chunks.Count < 200)
            chunks.Add(chunk);
        else
            chunk.Surface.Dispose();
    }
}
