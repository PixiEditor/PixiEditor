using ChunkyImageLib.DataHolders;
using System.Collections.Concurrent;

namespace ChunkyImageLib
{
    internal class SurfacePool
    {
        //must be divisible by 8
        public const int FullChunkSize = 256;

        private static object lockObj = new();
        private static SurfacePool? instance;
        public static SurfacePool Instance
        {
            get
            {
                if (instance is null)
                {
                    lock (lockObj)
                    {
                        if (instance is null)
                            instance = new SurfacePool();
                    }
                }
                return instance;
            }
        }

        private readonly ConcurrentBag<Surface> fullSurfaces = new();
        private readonly ConcurrentBag<Surface> halfSurfaces = new();
        private readonly ConcurrentBag<Surface> quarterSurfaces = new();
        private readonly ConcurrentBag<Surface> eighthSurfaces = new();
        internal Surface Get(ChunkResolution resolution)
        {
            if (GetBag(resolution).TryTake(out Surface? item))
                return item;
            return new Surface(new Vector2i(resolution.PixelSize(), resolution.PixelSize()));
        }

        private ConcurrentBag<Surface> GetBag(ChunkResolution resolution)
        {
            return resolution switch
            {
                ChunkResolution.Full => fullSurfaces,
                ChunkResolution.Half => halfSurfaces,
                ChunkResolution.Quarter => quarterSurfaces,
                ChunkResolution.Eighth => eighthSurfaces,
                _ => fullSurfaces
            };
        }

        internal void Push(Surface surface, ChunkResolution resolution)
        {
            var surfaces = GetBag(resolution);
            //a race condition can cause the count to go above 200, but likely not by much
            if (surfaces.Count < 200)
                surfaces.Add(surface);
            else
                surface.Dispose();
        }
    }
}
