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
    public Surface Surface { get; }
    public VecI PixelSize { get; }
    public ChunkResolution Resolution { get; }
    private Chunk(ChunkResolution resolution)
    {
        int size = resolution.PixelSize();

        Resolution = resolution;
        PixelSize = new(size, size);
        Surface = new Surface(PixelSize);
    }

    public static Chunk Create(ChunkResolution resolution = ChunkResolution.Full)
    {
        var chunk = ChunkPool.Instance.Get(resolution) ?? new Chunk(resolution);
        chunk.returned = false;
        Interlocked.Increment(ref chunkCounter);
        return chunk;
    }

    public void DrawOnSurface(SKSurface surface, VecI pos, SKPaint? paint = null)
    {
        surface.Canvas.DrawSurface(Surface.SkiaSurface, pos.X, pos.Y, paint);
    }

    public void Dispose()
    {
        if (returned)
            return;
        returned = true;
        Interlocked.Decrement(ref chunkCounter);
        ChunkPool.Instance.Push(this);
    }
}
