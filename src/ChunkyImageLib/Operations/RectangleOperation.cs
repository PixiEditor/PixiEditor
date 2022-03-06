using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib.Operations
{
    internal record class RectangleOperation : IOperation
    {
        public RectangleOperation(ShapeData rect)
        {
            Data = rect;
        }

        public ShapeData Data { get; }

        public void DrawOnChunk(Chunk chunk, int chunkX, int chunkY)
        {
            var skiaSurf = chunk.Surface.SkiaSurface;
            // use a clipping rectangle with 2x stroke width to make sure stroke doesn't stick outside rect bounds
            skiaSurf.Canvas.Save();
            var rect = SKRect.Create(Data.X - chunkX * ChunkPool.ChunkSize, Data.Y - chunkY * ChunkPool.ChunkSize, Data.Width, Data.Height);
            skiaSurf.Canvas.ClipRect(rect);

            // draw fill
            using SKPaint paint = new()
            {
                Color = Data.FillColor,
                Style = SKPaintStyle.Fill,
            };

            if (Data.FillColor.Alpha > 0)
                skiaSurf.Canvas.DrawRect(rect, paint);

            // draw stroke
            paint.Color = Data.StrokeColor;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = Data.StrokeWidth * 2;

            skiaSurf.Canvas.DrawRect(rect, paint);

            // get rid of the clipping rectangle
            skiaSurf.Canvas.Restore();
        }

        public HashSet<(int, int)> FindAffectedChunks()
        {
            if (Data.Width < 1 || Data.Height < 1 || Data.StrokeColor.Alpha == 0 && Data.FillColor.Alpha == 0)
                return new();
            if (Data.FillColor.Alpha != 0 || Data.Width == 1 || Data.Height == 1)
                return GetChunksForFilled(ChunkPool.ChunkSize);
            return GetChunksForStroke(ChunkPool.ChunkSize);
        }

        private static (int, int)? Inset(int min, int max, int inset)
        {
            int insetMin = Math.Min(min + inset - 1, max);
            int insetMax = Math.Max(max - inset + 1, min);
            //is rectangle fully filled by the stroke
            if (insetMin + 1 >= insetMax)
                return null;
            return (insetMin, insetMax);
        }

        private HashSet<(int, int)> GetChunksForStroke(int chunkSize)
        {
            //we need to account for wide strokes covering multiple chunks
            //find inner stroke boudaries in pixel coords
            var xInset = Inset(Data.X, Data.MaxX, Data.StrokeWidth);
            var yInset = Inset(Data.Y, Data.MaxY, Data.StrokeWidth);
            if (xInset == null || yInset == null)
                return GetChunksForFilled(chunkSize);

            //find two chunk rectanges, outer and inner
            var (minX, minY) = OperationHelper.GetChunkPos(Data.X, Data.Y, chunkSize);
            var (maxX, maxY) = OperationHelper.GetChunkPos(Data.MaxX, Data.MaxY, chunkSize);
            var (minInsetX, minInsetY) = OperationHelper.GetChunkPos(xInset.Value.Item1, yInset.Value.Item1, chunkSize);
            var (maxInsetX, maxInsetY) = OperationHelper.GetChunkPos(xInset.Value.Item2, yInset.Value.Item2, chunkSize);

            //fill in sides
            HashSet<(int, int)> chunks = new();
            AddRectangle(minX, minY, maxX, minInsetY, chunks); //top
            AddRectangle(minX, minInsetY + 1, minInsetX, maxInsetY - 1, chunks); //left
            AddRectangle(maxInsetX, minInsetY + 1, maxX, maxInsetY - 1, chunks); //right
            AddRectangle(minX, maxInsetY, maxX, maxY, chunks); //bottom
            return chunks;
        }

        private HashSet<(int, int)> GetChunksForFilled(int chunkSize)
        {
            var (minX, minY) = OperationHelper.GetChunkPos(Data.X, Data.Y, chunkSize);
            var (maxX, maxY) = OperationHelper.GetChunkPos(Data.MaxX, Data.MaxY, chunkSize);
            HashSet<(int, int)> output = new();
            AddRectangle(minX, minY, maxX, maxY, output);
            return output;
        }

        private static void AddRectangle(int minX, int minY, int maxX, int maxY, HashSet<(int, int)> set)
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    set.Add((x, y));
                }
            }
        }

        public void Dispose() { }
    }
}
