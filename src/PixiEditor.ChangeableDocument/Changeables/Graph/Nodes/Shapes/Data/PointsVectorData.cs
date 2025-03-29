using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using Drawie.Numerics.Helpers;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public class PointsVectorData : ShapeVectorData
{
    public List<VecD> Points { get; set; } = new();

    public PointsVectorData(IEnumerable<VecD> points)
    {
        Points = new List<VecD>(points);
    }

    public override RectD GeometryAABB => new RectD(Points.Min(p => p.X), Points.Min(p => p.Y), Points.Max(p => p.X),
        Points.Max(p => p.Y));

    public override RectD VisualAABB => GeometryAABB;

    public override ShapeCorners TransformationCorners => new ShapeCorners(
        GeometryAABB).WithMatrix(TransformationMatrix);

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
        using Paint paint = new Paint();
        paint.SetPaintable(FillPaintable);
        paint.StrokeWidth = StrokeWidth;

        int num = 0;
        if (applyTransform)
        {
            num = canvas.Save();
            Matrix3X3 final = TransformationMatrix;
            canvas.SetMatrix(final);
        }

        canvas.DrawPoints(PointMode.Points, Points.ToVecFArray(), paint);

        if (applyTransform)
        {
            canvas.RestoreToCount(num);
        }
    }

    public override bool IsValid()
    {
        return Points.Count > 0;
    }

    protected override int GetSpecificHash()
    {
        HashCode hash = new();
        hash.Add(Points);
        return hash.ToHashCode();
    }

    protected override void AdjustCopy(ShapeVectorData copy)
    {
        if (copy is PointsVectorData pointsVectorData)
        {
            pointsVectorData.Points = new List<VecD>(Points);
        }
    }

    public override VectorPath ToPath(bool transformed = false)
    {
        VectorPath path = new VectorPath();

        foreach (VecD point in Points)
        {
            path.LineTo((VecF)point);
        }

        if (transformed)
        {
            path.Transform(TransformationMatrix);
        }

        return path;
    }

    protected bool Equals(PointsVectorData other)
    {
        return base.Equals(other) && (Points.Equals(other.Points) || Points.SequenceEqual(other.Points));
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

        return Equals((PointsVectorData)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Points);
    }
}
