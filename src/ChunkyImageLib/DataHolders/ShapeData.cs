using SkiaSharp;

namespace ChunkyImageLib.DataHolders;

public record struct ShapeData
{
    public ShapeData(Vector2d center, Vector2d size, double rotation, int strokeWidth, SKColor strokeColor, SKColor fillColor, SKBlendMode blendMode = SKBlendMode.SrcOver)
    {
        StrokeColor = strokeColor;
        FillColor = fillColor;
        Center = center;
        Size = size;
        Angle = rotation;
        StrokeWidth = strokeWidth;
        BlendMode = blendMode;
    }
    public SKColor StrokeColor { get; }
    public SKColor FillColor { get; }
    public SKBlendMode BlendMode { get; }
    public Vector2d Center { get; }
    public Vector2d Size { get; }
    public double Angle { get; }
    public int StrokeWidth { get; }
}
