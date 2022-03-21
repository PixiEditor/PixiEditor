using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib.Operations
{
    internal record class ImageOperation : IDrawOperation
    {
        private Vector2i pos;
        private Surface toPaint;
        private static SKPaint ReplacingPaint = new() { BlendMode = SKBlendMode.Src };

        public bool IgnoreEmptyChunks => false;

        public ImageOperation(Vector2i pos, Surface image)
        {
            this.pos = pos;
            toPaint = new Surface(image);
        }

        public void DrawOnChunk(Chunk chunk, Vector2i chunkPos)
        {
            chunk.Surface.SkiaSurface.Canvas.DrawSurface(toPaint.SkiaSurface, pos - chunkPos * ChunkPool.FullChunkSize, ReplacingPaint);
        }

        public HashSet<Vector2i> FindAffectedChunks()
        {
            Vector2i start = OperationHelper.GetChunkPos(pos, ChunkPool.FullChunkSize);
            Vector2i end = OperationHelper.GetChunkPos(new(pos.X + toPaint.Size.X - 1, pos.Y + toPaint.Size.Y - 1), ChunkPool.FullChunkSize);
            HashSet<Vector2i> output = new();
            for (int cx = start.X; cx <= end.X; cx++)
            {
                for (int cy = start.Y; cy <= end.Y; cy++)
                {
                    output.Add(new(cx, cy));
                }
            }
            return output;
        }

        public void Dispose()
        {
            toPaint.Dispose();
        }
    }
}
