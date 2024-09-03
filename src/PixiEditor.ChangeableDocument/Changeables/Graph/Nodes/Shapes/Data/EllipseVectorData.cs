using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public class EllipseVectorData : ShapeVectorData
{
    public VecD Radius
    {
        get => Size / 2;
        set => Size = value * 2;
    } 

    public override RectD AABB =>
        new ShapeCorners(Position, Size)
            .AABBBounds;
    
    public EllipseVectorData(VecD center, VecD radius)
    {
        Position = center;
        Radius = radius;
    }

    public override void Rasterize(DrawingSurface drawingSurface)
    {
        var imageSize = (VecI)Size; 

        using ChunkyImage img = new ChunkyImage(imageSize);
        RectI rect = new RectI(0, 0, imageSize.X, imageSize.Y); 
        
        img.EnqueueDrawEllipse(rect, StrokeColor, FillColor, StrokeWidth, RotationRadians);
        img.CommitChanges();
        
        VecI topLeft = new VecI((int)(Position.X - Radius.X), (int)(Position.Y - Radius.Y));

        img.DrawMostUpToDateRegionOn(rect, ChunkResolution.Full, drawingSurface, topLeft);
    }

    public override bool IsValid()
    {
        return Radius is { X: > 0, Y: > 0 };
    }

    public override int CalculateHash()
    {
        return HashCode.Combine(Position, Radius);
    }

    public override int GetCacheHash()
    {
        return CalculateHash();
    }

    public override object Clone()
    {
        return new EllipseVectorData(Position, Radius)
        {
            StrokeColor = StrokeColor,
            FillColor = FillColor,
            StrokeWidth = StrokeWidth
        };
    }
}
