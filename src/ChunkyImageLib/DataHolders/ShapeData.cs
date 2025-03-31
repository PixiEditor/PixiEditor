using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace ChunkyImageLib.DataHolders;

public record struct ShapeData
{
    public ShapeData(VecD center, VecD size, double cornerRadius, double rotation, float strokeWidth, Paintable stroke, Paintable fillPaintable, BlendMode blendMode = BlendMode.SrcOver)
    {
        Stroke = stroke;
        FillPaintable = fillPaintable;
        Center = center;
        Size = size;
        Angle = rotation;
        StrokeWidth = strokeWidth;
        CornerRadius = cornerRadius;
        BlendMode = blendMode;
    }
    public Paintable Stroke { get; }
    public Paintable FillPaintable { get; }
    public BlendMode BlendMode { get; }
    public VecD Center { get; }

    /// <summary>Can be negative to show flipping </summary>
    public VecD Size { get; }

    public double CornerRadius { get; }
    public double Angle { get; }
    public float StrokeWidth { get; }
    public bool AntiAliasing { get; set; } = false;
    

    public ShapeData AsMirroredAcrossHorAxis(double horAxisY)
        => new ShapeData(Center.ReflectY(horAxisY), new(Size.X, -Size.Y), CornerRadius, -Angle, StrokeWidth, Stroke, FillPaintable, BlendMode);
    public ShapeData AsMirroredAcrossVerAxis(double verAxisX)
        => new ShapeData(Center.ReflectX(verAxisX), new(-Size.X, Size.Y), CornerRadius, -Angle, StrokeWidth, Stroke, FillPaintable, BlendMode);

}
