using System.Diagnostics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public class LineVectorData(VecD startPos, VecD pos) : ShapeVectorData, IReadOnlyLineData
{
    public VecD Start { get; set; } = startPos; // Relative to the document top left
    public VecD End { get; set; } = pos; // Relative to the document top left

    public VecD TransformedStart
    {
        get => TransformationMatrix.MapPoint(Start);
        set => Start = TransformationMatrix.Invert().MapPoint(value);
    }

    public VecD TransformedEnd
    {
        get => TransformationMatrix.MapPoint(End);
        set => End = TransformationMatrix.Invert().MapPoint(value);
    }

    public override RectD GeometryAABB
    {
        get
        {
            return RectD.FromTwoPoints(Start, End).Inflate(StrokeWidth);
        }
    }

    public override ShapeCorners TransformationCorners => new ShapeCorners(GeometryAABB)
        .WithMatrix(TransformationMatrix);

    public override void RasterizeGeometry(DrawingSurface drawingSurface, ChunkResolution resolution, Paint? paint)
    {
        Rasterize(drawingSurface, paint, false);
    }

    public override void RasterizeTransformed(DrawingSurface drawingSurface, ChunkResolution resolution, Paint paint)
    {
        Rasterize(drawingSurface, paint, true);
    }

    private void Rasterize(DrawingSurface drawingSurface, Paint paint, bool applyTransform)
    {
        int num = 0;
        if (applyTransform)
        {
            num = drawingSurface.Canvas.Save();
            ApplyTransformTo(drawingSurface);
        }

        paint.Color = StrokeColor;
        paint.Style = PaintStyle.Stroke;
        paint.StrokeWidth = StrokeWidth;

        drawingSurface.Canvas.DrawLine(Start, End, paint);

        if (applyTransform)
        {
            drawingSurface.Canvas.RestoreToCount(num);
        }

        /*
        RectD adjustedAABB = GeometryAABB.RoundOutwards();
        adjustedAABB = adjustedAABB with { Size = adjustedAABB.Size + new VecD(1, 1) };
        var imageSize = (VecI)adjustedAABB.Size;

        using ChunkyImage img = new ChunkyImage(imageSize);

        if (StrokeWidth == 1)
        {
            VecD adjustment = new VecD(0.5, 0.5);

            img.EnqueueDrawBresenhamLine(
                (VecI)(Start - adjustedAABB.TopLeft - adjustment),
                (VecI)(End - adjustedAABB.TopLeft - adjustment), StrokeColor, BlendMode.SrcOver);
        }
        else
        {
            img.EnqueueDrawSkiaLine(
                (VecI)Start.Round() - (VecI)adjustedAABB.TopLeft,
                (VecI)End.Round() - (VecI)adjustedAABB.TopLeft, StrokeCap.Butt, StrokeWidth, StrokeColor, BlendMode.SrcOver);
        }

        img.CommitChanges();

        VecI topLeft = (VecI)(adjustedAABB.TopLeft * resolution.Multiplier());

        RectI region = new(VecI.Zero, imageSize);

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
    */
    }

    public override bool IsValid()
    {
        return Start != End;
    }

    public override int GetCacheHash()
    {
        return HashCode.Combine(Start, End, StrokeColor, StrokeWidth, TransformationMatrix);
    }

    public override int CalculateHash()
    {
        return GetCacheHash();
    }

    public override object Clone()
    {
        return new LineVectorData(Start, End)
        {
            StrokeColor = StrokeColor, StrokeWidth = StrokeWidth, TransformationMatrix = TransformationMatrix
        };
    }
}
