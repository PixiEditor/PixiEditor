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

    public override RectD AABB => new RectD(Points.Min(p => p.X), Points.Min(p => p.Y), Points.Max(p => p.X), Points.Max(p => p.Y));

    public override void Rasterize(DrawingSurface drawingSurface)
    {
        using Paint paint = new Paint();
        paint.Color = FillColor;
        paint.StrokeWidth = StrokeWidth;
        
        drawingSurface.Canvas.DrawPoints(PointMode.Points, Points.Select(p => new Point((int)p.X, (int)p.Y)).ToArray(), paint);
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
            StrokeColor = StrokeColor,
            FillColor = FillColor,
            StrokeWidth = StrokeWidth
        };
    }
}
