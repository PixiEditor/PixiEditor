using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib
{
    public class Chunk : IDisposable, IReadOnlyChunk
    {
        private bool returned = false;
        public Surface Surface { get; }
        public Vector2i PixelSize { get; }
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
            return chunk;
        }

        public void DrawOnSurface(SKSurface surface, Vector2i pos, SKPaint? paint = null)
        {
            surface.Canvas.DrawSurface(Surface.SkiaSurface, pos.X, pos.Y, paint);
        }

        public void Dispose()
        {
            if (returned)
                return;
            returned = true;
            ChunkPool.Instance.Push(this);
        }
    }
}
