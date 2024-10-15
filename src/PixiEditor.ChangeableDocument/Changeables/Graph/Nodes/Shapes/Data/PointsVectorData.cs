using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

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

    public override ShapeCorners TransformationCorners => new ShapeCorners(
        GeometryAABB).WithMatrix(TransformationMatrix);

    public override void RasterizeGeometry(DrawingSurface drawingSurface)
    {
        Rasterize(drawingSurface, false);
    }

    public override void RasterizeTransformed(DrawingSurface drawingSurface)
    {
        Rasterize(drawingSurface, true);
    }

    private void Rasterize(DrawingSurface drawingSurface, bool applyTransform)
    {
        using Paint paint = new Paint();
        paint.Color = FillColor;
        paint.StrokeWidth = StrokeWidth;

        int num = 0;
        if (applyTransform)
        {
            num = drawingSurface.Canvas.Save();
            Matrix3X3 final = TransformationMatrix;
            drawingSurface.Canvas.SetMatrix(final);
        }

        drawingSurface.Canvas.DrawPoints(PointMode.Points, Points.Select(p => new Point((int)p.X, (int)p.Y)).ToArray(),
            paint);

        if (applyTransform)
        {
            drawingSurface.Canvas.RestoreToCount(num);
        }
    }

    public override bool IsValid()
    {
        return Points.Count > 0;
    }

    public override int GetCacheHash()
    {
        return CalculateHash();
    }

    public override int CalculateHash()
    {
        return Points.GetHashCode();
    }

    public override object Clone()
    {
        return new PointsVectorData(Points)
        {
            StrokeColor = StrokeColor, FillColor = FillColor, StrokeWidth = StrokeWidth
        };
    }
}
