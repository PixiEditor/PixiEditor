using SkiaSharp;

namespace ChunkyImageLib.DataHolders
{
    public record ShapeData
    {
        public ShapeData(int x, int y, int width, int height, int strokeWidth, SKColor strokeColor, SKColor fillColor)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            StrokeColor = strokeColor;
            FillColor = fillColor;
            StrokeWidth = strokeWidth;
        }

        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
        public SKColor StrokeColor { get; }
        public SKColor FillColor { get; }
        public int StrokeWidth { get; }
        public int MaxX => X + Width - 1;
        public int MaxY => Y + Height - 1;
    }
}
