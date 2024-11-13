using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace ChunkyImageLib.DataHolders;

public record struct ShapeData
{
    public ShapeData(VecD center, VecD size, double rotation, int strokeWidth, Color strokeColor, Color fillColor, BlendMode blendMode = BlendMode.SrcOver)
    {
        StrokeColor = strokeColor;
        FillColor = fillColor;
        Center = center;
        Size = size;
        Angle = rotation;
        StrokeWidth = strokeWidth;
        BlendMode = blendMode;
    }
    public Color StrokeColor { get; }
    public Color FillColor { get; }
    public BlendMode BlendMode { get; }
    public VecD Center { get; }

    /// <summary>Can be negative to show flipping </summary>
    public VecD Size { get; }
    public double Angle { get; }
    public int StrokeWidth { get; }

    public bool AntiAliasing { get; set; } = false;
    

    public ShapeData AsMirroredAcrossHorAxis(double horAxisY)
        => new ShapeData(Center.ReflectY(horAxisY), new(Size.X, -Size.Y), -Angle, StrokeWidth, StrokeColor, FillColor, BlendMode);
    public ShapeData AsMirroredAcrossVerAxis(double verAxisX)
        => new ShapeData(Center.ReflectX(verAxisX), new(-Size.X, Size.Y), -Angle, StrokeWidth, StrokeColor, FillColor, BlendMode);

}
