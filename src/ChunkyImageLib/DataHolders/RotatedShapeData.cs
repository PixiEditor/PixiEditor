using SkiaSharp;

namespace ChunkyImageLib.DataHolders;

public record struct RotatedShapeData
{
    public RotatedShapeData(Vector2d center, Vector2d size, float angle, int strokeWidth, SKColor strokeColor, SKColor fillColor)
    {
        Center = center;
        Size = size;
        Angle = angle;
        StrokeColor = strokeColor;
        FillColor = fillColor;
        StrokeWidth = strokeWidth;
    }

    public Vector2d Center { get; }
    public float Angle { get; }
    public Vector2d Size { get; }
    public SKColor StrokeColor { get; }
    public SKColor FillColor { get; }
    public int StrokeWidth { get; }
}
