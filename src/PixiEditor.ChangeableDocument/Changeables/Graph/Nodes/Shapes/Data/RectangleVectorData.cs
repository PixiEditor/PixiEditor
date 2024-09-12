using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.Vector;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public class RectangleVectorData : ShapeVectorData, IReadOnlyRectangleData
{
    public VecD Center { get; }
    public VecD Size { get; }

    public override RectD GeometryAABB => RectD.FromCenterAndSize(Center, Size); 

    public override ShapeCorners TransformationCorners =>
        new ShapeCorners(Center, Size).WithMatrix(TransformationMatrix);


    public RectangleVectorData(VecD center, VecD size)
    {
        Center = center;
        Size = size;
    }
    
    public override void Rasterize(DrawingSurface drawingSurface, ChunkResolution resolution)
    {
        var imageSize = (VecI)Size; 

        using ChunkyImage img = new ChunkyImage(imageSize);

        RectI drawRect = (RectI)RectD.FromTwoPoints(VecD.Zero, Size).RoundOutwards();

        ShapeData data = new ShapeData(drawRect.Center, drawRect.Size, 0, StrokeWidth, StrokeColor, FillColor);
        img.EnqueueDrawRectangle(data);
        img.CommitChanges();

        VecI topLeft = (VecI)(Center - Size / 2); 

        RectI region = new(VecI.Zero, (VecI)GeometryAABB.Size);

        int num = drawingSurface.Canvas.Save();
        drawingSurface.Canvas.SetMatrix(TransformationMatrix);

        img.DrawMostUpToDateRegionOn(region, resolution, drawingSurface, topLeft);

        drawingSurface.Canvas.RestoreToCount(num);
    }

    public override bool IsValid()
    {
        return Size is { X: > 0, Y: > 0 };
    }

    public override int CalculateHash()
    {
        return HashCode.Combine(Center, Size, StrokeColor, FillColor, StrokeWidth, TransformationMatrix);
    }

    public override int GetCacheHash()
    {
        return CalculateHash();
    }

    public override object Clone()
    {
        return new RectangleVectorData(Center, Size)
        {
            StrokeColor = StrokeColor,
            FillColor = FillColor,
            StrokeWidth = StrokeWidth,
            TransformationMatrix = TransformationMatrix
        };
    }
}
