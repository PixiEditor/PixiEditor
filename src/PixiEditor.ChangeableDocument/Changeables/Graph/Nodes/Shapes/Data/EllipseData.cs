using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public class EllipseData : ShapeData
{
    public VecD Center { get; set; }
    public VecD Radius { get; set; }

    public EllipseData(VecD center, VecD radius)
    {
        Center = center;
        Radius = radius;
    }
    
    public override void Rasterize(DrawingSurface drawingSurface)
    {
        var imageSize = new VecI((int)Radius.X * 2, (int)Radius.Y * 2);

        using ChunkyImage img = new ChunkyImage(imageSize);
        RectI rect = new RectI(0, 0, (int)Radius.X * 2, (int)Radius.Y * 2);
        
        img.EnqueueDrawEllipse(rect, StrokeColor, FillColor, StrokeWidth);
        img.CommitChanges();
        
        VecI pos = new VecI((int)(Center.X - Radius.X), (int)(Center.Y - Radius.Y));
        img.DrawMostUpToDateRegionOn(rect, ChunkResolution.Full, drawingSurface, pos);
    }

    public override bool IsValid()
    {
        return Radius is { X: > 0, Y: > 0 };
    }

    public override int CalculateHash()
    {
        return HashCode.Combine(Center, Radius);
    }

    public override int GetCacheHash()
    {
        return CalculateHash();
    }

    public override object Clone()
    {
        return new EllipseData(Center, Radius)
        {
            StrokeColor = StrokeColor,
            FillColor = FillColor,
            StrokeWidth = StrokeWidth
        };
    }
}
