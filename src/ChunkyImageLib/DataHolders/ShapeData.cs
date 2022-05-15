using SkiaSharp;

namespace ChunkyImageLib.DataHolders;

public record struct ShapeData
{
    public ShapeData(VecD center, VecD size, double rotation, int strokeWidth, SKColor strokeColor, SKColor fillColor, SKBlendMode blendMode = SKBlendMode.SrcOver)
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
    public VecD Center { get; }
    /// <summary>Can be negative to show flipping </summary>
    public VecD Size { get; }
    public double Angle { get; }
    public int StrokeWidth { get; }

    public ShapeData AsMirroredAcrossHorAxis(int horAxisY)
        => new ShapeData(Center.ReflectY(horAxisY), new(Size.X, -Size.Y), -Angle, StrokeWidth, StrokeColor, FillColor, BlendMode);
    public ShapeData AsMirroredAcrossVerAxis(int verAxisX)
        => new ShapeData(Center.ReflectX(verAxisX), new(-Size.X, Size.Y), -Angle, StrokeWidth, StrokeColor, FillColor, BlendMode);

}
