using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib;

public class Chunk : IDisposable
{
    private static volatile int chunkCounter = 0;
    /// <summary>
    /// The number of chunks that haven't yet been returned (includes garbage collected chunks).
    /// Used in tests to make sure that all chunks are disposed.
    /// </summary>
    public static int ChunkCounter => chunkCounter;

    private bool returned = false;
    /// <summary>
    /// The surface of the chunk
    /// </summary>
    public Surface Surface { get; }
    /// <summary>
    /// The size of the chunk
    /// </summary>
    public VecI PixelSize { get; }
    /// <summary>
    /// The resolution of the chunk
    /// </summary>
    public ChunkResolution Resolution { get; }
    private Chunk(ChunkResolution resolution)
    {
        int size = resolution.PixelSize();

        Resolution = resolution;
        PixelSize = new(size, size);
        Surface = new Surface(PixelSize);
    }

    /// <summary>
    /// Tries to take a chunk with the <paramref name="resolution"/> from the pool, or creates a new one
    /// </summary>
    public static Chunk Create(ChunkResolution resolution = ChunkResolution.Full)
    {
        var chunk = ChunkPool.Instance.Get(resolution) ?? new Chunk(resolution);
        chunk.returned = false;
        Interlocked.Increment(ref chunkCounter);
        return chunk;
    }

    /// <summary>
    /// Draw's on the <see cref="Surface"/> of the chunk
    /// </summary>
    /// <param name="pos">The destination for the <paramref name="surface"/></param>
    /// <param name="paint">The paint to use while drawing</param>
    public void DrawOnSurface(SKSurface surface, VecI pos, SKPaint? paint = null)
    {
        surface.Canvas.DrawSurface(Surface.SkiaSurface, pos.X, pos.Y, paint);
    }

    /// <summary>
    /// Returns the chunk back to the pool
    /// </summary>
    public void Dispose()
    {
        if (returned)
            return;
        returned = true;
        Interlocked.Decrement(ref chunkCounter);
        ChunkPool.Instance.Push(this);
    }
}
