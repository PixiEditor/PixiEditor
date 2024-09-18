using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
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

    public override void RasterizeGeometry(DrawingSurface drawingSurface, ChunkResolution resolution, Paint? paint)
    {
        Rasterize(drawingSurface, resolution, paint, false);
    }

    public override void RasterizeTransformed(DrawingSurface drawingSurface, ChunkResolution resolution, Paint paint)
    {
        Rasterize(drawingSurface, resolution, paint, true);
    }

    private void Rasterize(DrawingSurface drawingSurface, ChunkResolution resolution, Paint paint, bool applyTransform)
    {
        var imageSize = (VecI)Size; 

        using ChunkyImage img = new ChunkyImage(imageSize);

        RectI drawRect = (RectI)RectD.FromTwoPoints(VecD.Zero, Size).RoundOutwards();

        ShapeData data = new ShapeData(drawRect.Center, drawRect.Size, 0, StrokeWidth, StrokeColor, FillColor);
        img.EnqueueDrawRectangle(data);
        img.CommitChanges();
        
        VecI topLeft = (VecI)((Center - Size / 2) * resolution.Multiplier());
        RectI region = new(VecI.Zero, (VecI)GeometryAABB.Size);

        int num = 0;
        if (applyTransform)
        {
            num = drawingSurface.Canvas.Save();
            Matrix3X3 final = TransformationMatrix with
            {
                TransX = TransformationMatrix.TransX * (float)resolution.Multiplier(),
                TransY = TransformationMatrix.TransY * (float)resolution.Multiplier()
            };
            drawingSurface.Canvas.SetMatrix(final);
        }

        img.DrawMostUpToDateRegionOn(region, resolution, drawingSurface, topLeft, paint);
        
        if (applyTransform)
        {
            drawingSurface.Canvas.RestoreToCount(num);
        }
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
