using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib
{
    public class Chunk : IDisposable
    {
        private bool returned = false;
        internal Surface Surface { get; }
        public Vector2i PixelSize { get; }
        public ChunkResolution Resolution { get; }
        private Chunk(ChunkResolution resolution)
        {
            int size = resolution switch
            {
                ChunkResolution.Full => ChunkPool.FullChunkSize,
                ChunkResolution.Half => ChunkPool.FullChunkSize / 2,
                ChunkResolution.Quarter => ChunkPool.FullChunkSize / 4,
                ChunkResolution.Eighth => ChunkPool.FullChunkSize / 8,
                _ => ChunkPool.FullChunkSize
            };

            Resolution = resolution;
            Surface = new Surface(size, size);
            PixelSize = new(size, size);
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
