using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib
{
    public class Chunk : IDisposable
    {
        internal Surface Surface { get; }
        internal Chunk()
        {
            Surface = new Surface(ChunkPool.ChunkSize, ChunkPool.ChunkSize);
        }

        public void DrawOnSurface(SKSurface surface, Vector2i pos, SKPaint? paint = null)
        {
            surface.Canvas.DrawSurface(Surface.SkiaSurface, pos.X, pos.Y, paint);
        }

        public void Dispose()
        {
            Surface.Dispose();
        }
    }
}
