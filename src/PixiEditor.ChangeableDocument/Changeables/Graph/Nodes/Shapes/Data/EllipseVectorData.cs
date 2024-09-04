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
        new ShapeCorners(Position, Size).AsRotated(RotationRadians, Position)
            .AABBBounds;
    
    public EllipseVectorData(VecD center, VecD radius)
    {
        Position = center;
        Radius = radius;
    }

    public override void Rasterize(DrawingSurface drawingSurface)
    {
        var imageSize = (VecI)Size;
        
        using ChunkyImage img = new ChunkyImage((VecI)AABB.Size);

        RectD rotated = new ShapeCorners(RectD.FromTwoPoints(VecD.Zero, imageSize))
            .AsRotated(RotationRadians, imageSize / 2f).AABBBounds;

        VecI shift = new VecI(-(int)rotated.Left, -(int)rotated.Top);
        RectI drawRect = new(shift, imageSize);
        
        img.EnqueueDrawEllipse(drawRect, StrokeColor, FillColor, StrokeWidth, RotationRadians);
        img.CommitChanges();

        VecI topLeft = new VecI((int)(Position.X - Radius.X), (int)(Position.Y - Radius.Y)) - shift;
        
        RectI region = new(VecI.Zero, (VecI)AABB.Size);

        img.DrawMostUpToDateRegionOn(region, ChunkResolution.Full, drawingSurface, topLeft);
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
