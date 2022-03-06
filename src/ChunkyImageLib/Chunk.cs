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

        public SKImage Snapshot()
        {
            return Surface.SkiaSurface.Snapshot();
        }

        public void Dispose()
        {
            Surface.Dispose();
        }
    }
}
