using SkiaSharp;

namespace ChunkyImageLib.Operations
{
    internal record RectangleOperation : IOperation
    {
        public RectangleOperation(int x, int y, int width, int height, int borderThickness, SKColor borderColor, SKColor fillColor)
        {
            StrokeColor = borderColor;
            FillColor = fillColor;
            StrokeWidth = borderThickness;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public SKColor StrokeColor { get; }
        public SKColor FillColor { get; }
        public int StrokeWidth { get; }
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
        public int MaxX => X + Width - 1;
        public int MaxY => Y + Height - 1;

        public void DrawOnChunk(ImageData chunk, int chunkX, int chunkY)
        {
            // use a clipping rectangle with 2x stroke width to make sure stroke doesn't stick outside rect bounds
            chunk.SkiaSurface.Canvas.Save();
            var rect = SKRect.Create(X, Y, Width, Height);
            chunk.SkiaSurface.Canvas.ClipRect(rect);

            // draw fill
            using SKPaint paint = new()
            {
                Color = FillColor,
                Style = SKPaintStyle.Fill,
            };

            if (FillColor.Alpha > 0)
                chunk.SkiaSurface.Canvas.DrawRect(rect, paint);

            // draw stroke
            paint.Color = StrokeColor;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = StrokeWidth * 2;

            chunk.SkiaSurface.Canvas.DrawRect(rect, paint);

            // get rid of the clipping rectangle
            chunk.SkiaSurface.Canvas.Restore();
        }

        public HashSet<(int, int)> FindAffectedChunks(int chunkSize)
        {
            if (Width < 1 || Height < 1 || StrokeColor.Alpha == 0 && FillColor.Alpha == 0)
                return new();
            if (FillColor.Alpha != 0 || Width == 1 || Height == 1)
                return GetChunksForFilled(chunkSize);
            return GetChunksForStroke(chunkSize);
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
            var xInset = Inset(X, MaxX, StrokeWidth);
            var yInset = Inset(Y, MaxY, StrokeWidth);
            if (xInset == null || yInset == null)
                return GetChunksForFilled(chunkSize);

            //find two chunk rectanges, outer and inner
            var (minX, minY) = OperationHelper.GetChunkPos(X, Y, chunkSize);
            var (maxX, maxY) = OperationHelper.GetChunkPos(MaxX, MaxY, chunkSize);
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
            var (minX, minY) = OperationHelper.GetChunkPos(X, Y, chunkSize);
            var (maxX, maxY) = OperationHelper.GetChunkPos(MaxX, MaxY, chunkSize);
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
    }
}
