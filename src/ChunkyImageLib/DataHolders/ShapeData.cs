using SkiaSharp;

namespace ChunkyImageLib.DataHolders;

public record struct ShapeData
{
    public ShapeData(Vector2i pos, Vector2i size, int strokeWidth, SKColor strokeColor, SKColor fillColor, SKBlendMode blendMode = SKBlendMode.SrcOver)
    {
        Pos = pos;
        MaxPos = new(pos.X + size.X - 1, pos.Y + size.Y - 1);
        Size = size;
        StrokeColor = strokeColor;
        FillColor = fillColor;
        StrokeWidth = strokeWidth;
        BlendMode = blendMode;
    }

    public Vector2i Pos { get; }
    public Vector2i MaxPos { get; }
    public Vector2i Size { get; }
    public SKColor StrokeColor { get; }
    public SKColor FillColor { get; }
    public SKBlendMode BlendMode { get; }
    public int StrokeWidth { get; }
}
