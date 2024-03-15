using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

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

    public bool IsDirty { get; set; }

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

    public Guid Uid { get; } = Guid.NewGuid();

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
    public void DrawOnSurface(DrawingSurface surface, VecI pos, Paint? paint = null)
    {
        surface.Canvas.DrawSurface(Surface.DrawingSurface, pos.X, pos.Y, paint);
    }
    
    public unsafe RectI? FindPreciseBounds(RectI? passedSearchRegion = null)
    {
        RectI? bounds = null;
        if (returned) 
            return bounds;

        if (passedSearchRegion is not null && !new RectI(VecI.Zero, Surface.Size).ContainsInclusive(passedSearchRegion.Value))
            throw new ArgumentException("Passed search region lies outside of the chunk's surface", nameof(passedSearchRegion));

        RectI searchRegion = passedSearchRegion ?? new RectI(VecI.Zero, Surface.Size);
        
        ulong* ptr = (ulong*)Surface.PixelBuffer;
        for (int y = searchRegion.Top; y < searchRegion.Bottom; y++)
        {
            for (int x = searchRegion.Left; x < searchRegion.Right; x++)
            {
                int i = y * Surface.Size.X + x;
                // ptr[i] actually contains 4 16-bit floats. We only care about the first one which is alpha.
                // An empty pixel can have alpha of 0 or -0 (not sure if -0 actually ever comes up). 0 in hex is 0x0, -0 in hex is 0x8000
                if ((ptr[i] & 0x1111_0000_0000_0000) != 0 && (ptr[i] & 0x1111_0000_0000_0000) != 0x8000_0000_0000_0000)
                {
                    bounds ??= new RectI(x, y, 1, 1);
                    bounds = bounds.Value.Union(new RectI(x, y, 1, 1));
                }
            }
        }
        
        return bounds;
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
        Surface.DrawingSurface.Canvas.RestoreToCount(-1);
        ChunkPool.Instance.Push(this);
    }
}
