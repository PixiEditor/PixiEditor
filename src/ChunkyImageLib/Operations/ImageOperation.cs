using SkiaSharp;

namespace ChunkyImageLib.Operations
{
    internal record class ImageOperation : IOperation
    {
        private int x;
        private int y;
        private Surface toPaint;
        private static SKPaint ReplacingPaint = new() { BlendMode = SKBlendMode.Src };
        public ImageOperation(int x, int y, Surface image)
        {
            this.x = x;
            this.y = y;
            toPaint = new Surface(image);
        }

        public void DrawOnChunk(Chunk chunk, int chunkX, int chunkY)
        {
            chunk.Surface.SkiaSurface.Canvas.DrawSurface(toPaint.SkiaSurface, x - chunkX * ChunkPool.ChunkSize, y - chunkY * ChunkPool.ChunkSize, ReplacingPaint);
        }

        public HashSet<(int, int)> FindAffectedChunks()
        {
            var (startX, startY) = OperationHelper.GetChunkPos(x, y, ChunkPool.ChunkSize);
            var (endX, endY) = OperationHelper.GetChunkPos(x + toPaint.Width - 1, y + toPaint.Height - 1, ChunkPool.ChunkSize);
            HashSet<(int, int)> output = new();
            for (int cx = startX; cx <= endX; cx++)
            {
                for (int cy = startY; cy <= endY; cy++)
                {
                    output.Add((cx, cy));
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
