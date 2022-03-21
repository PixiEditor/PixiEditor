using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib.Operations
{
    internal class ClearRegionOperation : IDrawOperation
    {
        Vector2i pos;
        Vector2i size;

        public bool IgnoreEmptyChunks => true;

        public ClearRegionOperation(Vector2i pos, Vector2i size)
        {
            this.pos = pos;
            this.size = size;
        }

        public void DrawOnChunk(Chunk chunk, Vector2i chunkPos)
        {
            chunk.Surface.SkiaSurface.Canvas.Save();
            chunk.Surface.SkiaSurface.Canvas.ClipRect(SKRect.Create(pos - chunkPos * ChunkPool.FullChunkSize, size));
            chunk.Surface.SkiaSurface.Canvas.Clear();
            chunk.Surface.SkiaSurface.Canvas.Restore();
        }

        public HashSet<Vector2i> FindAffectedChunks()
        {
            return OperationHelper.FindChunksFullyInsideRectangle(pos, size);
        }
        public void Dispose() { }
    }
}
