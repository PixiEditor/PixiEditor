using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public class EllipseVectorData : ShapeVectorData, IReadOnlyEllipseData
{
    public VecD Radius { get; set; }

    public VecD Center { get; set; }

    public override RectD GeometryAABB =>
        new ShapeCorners(Center, Radius * 2).AABBBounds;

    public override RectD VisualAABB =>
        RectD.FromCenterAndSize(Center, Radius * 2).Inflate(StrokeWidth / 2);

    public override ShapeCorners TransformationCorners =>
        new ShapeCorners(Center, Radius * 2).WithMatrix(TransformationMatrix);


    public EllipseVectorData(VecD center, VecD radius)
    {
        Center = center;
        Radius = radius;
    }

    public override void RasterizeGeometry(Canvas drawingSurface)
    {
        Rasterize(drawingSurface, false);
    }

    public override void RasterizeTransformed(Canvas drawingSurface)
    {
        Rasterize(drawingSurface, true);
    }

    private void Rasterize(Canvas canvas, bool applyTransform)
    {
        int saved = 0;
        if (applyTransform)
        {
            saved = canvas.Save();
            ApplyTransformTo(canvas);
        }

        using Paint shapePaint = new Paint();
        shapePaint.IsAntiAliased = true;

        if (Fill)
        {
            shapePaint.SetPaintable(FillPaintable);
            shapePaint.Style = PaintStyle.Fill;
            canvas.DrawOval(Center, Radius, shapePaint);
        }

        if (StrokeWidth > 0)
        {
            shapePaint.SetPaintable(Stroke);
            shapePaint.Style = PaintStyle.Stroke;
            shapePaint.StrokeWidth = StrokeWidth;
            canvas.DrawOval(Center, Radius, shapePaint);
        }

        if (applyTransform)
        {
            canvas.RestoreToCount(saved);
        }
    }

    public override bool IsValid()
    {
        return Radius is { X: > 0, Y: > 0 };
    }

    protected override int GetSpecificHash()
    {
        return HashCode.Combine(Center, Radius);
    }

    protected override void AdjustCopy(ShapeVectorData copy)
    {
    }

    public override VectorPath ToPath(bool transformed = false)
    {
        VectorPath path = new VectorPath();
        path.AddOval(RectD.FromCenterAndSize(Center, Radius * 2));
        if (transformed)
        {
            path.Transform(TransformationMatrix);
        }

        return path;
    }

    protected bool Equals(EllipseVectorData other)
    {
        return base.Equals(other) && Radius.Equals(other.Radius) && Center.Equals(other.Center);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((EllipseVectorData)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Radius, Center);
    }
}
