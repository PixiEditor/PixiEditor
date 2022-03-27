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
            Vector2i convPos = OperationHelper.ConvertForResolution(pos, chunk.Resolution);
            Vector2i convSize = OperationHelper.ConvertForResolution(size, chunk.Resolution);

            chunk.Surface.SkiaSurface.Canvas.Save();
            chunk.Surface.SkiaSurface.Canvas.ClipRect(SKRect.Create(convPos - chunkPos.Multiply(chunk.PixelSize), convSize));
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
