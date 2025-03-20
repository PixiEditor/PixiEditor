using ChunkyImageLib.DataHolders;
using System.Collections.Concurrent;
using Drawie.Backend.Core.Surfaces.ImageData;

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

    private readonly ConcurrentDictionary<ColorSpace, ConcurrentBag<Chunk>> fullChunks = new();
    private readonly ConcurrentDictionary<ColorSpace, ConcurrentBag<Chunk>> halfChunks = new();
    private readonly ConcurrentDictionary<ColorSpace, ConcurrentBag<Chunk>> quarterChunks = new();
    private readonly ConcurrentDictionary<ColorSpace, ConcurrentBag<Chunk>> eighthChunks = new();

    /// <summary>
    /// Tries to take a chunk from the pool, returns null if there's no Chunk available
    /// </summary>
    /// <param name="resolution">The resolution for the chunk</param>
    /// <param name="chunkCs"></param>
    internal Chunk? Get(ChunkResolution resolution, ColorSpace chunkCs) => GetBag(resolution, chunkCs).TryTake(out Chunk? item) ? item : null;

    private ConcurrentBag<Chunk> GetBag(ChunkResolution resolution, ColorSpace colorSpace)
    {
        return resolution switch
        {
            ChunkResolution.Full => fullChunks.GetOrAdd(colorSpace, _ => new ConcurrentBag<Chunk>()),
            ChunkResolution.Half => halfChunks.GetOrAdd(colorSpace, _ => new ConcurrentBag<Chunk>()),
            ChunkResolution.Quarter => quarterChunks.GetOrAdd(colorSpace, _ => new ConcurrentBag<Chunk>()),
            ChunkResolution.Eighth => eighthChunks.GetOrAdd(colorSpace, _ => new ConcurrentBag<Chunk>()),
            _ => fullChunks.GetOrAdd(colorSpace, _ => new ConcurrentBag<Chunk>()),
        };
    }

    /// <summary>
    /// Returns a chunk back to the pool
    /// </summary>
    internal void Push(Chunk chunk)
    {
        var chunks = GetBag(chunk.Resolution, chunk.ColorSpace);
        //a race condition can cause the count to go above 200, but likely not by much
        if (chunks.Count < 200)
            chunks.Add(chunk);
        else
            chunk.Surface.Dispose();
    }
}
