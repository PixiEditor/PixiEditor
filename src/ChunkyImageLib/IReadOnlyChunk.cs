using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib
{
    public interface IReadOnlyChunk
    {
        Vector2i PixelSize { get; }
        ChunkResolution Resolution { get; }
        void DrawOnSurface(SKSurface surface, Vector2i pos, SKPaint? paint = null);
    }
}
