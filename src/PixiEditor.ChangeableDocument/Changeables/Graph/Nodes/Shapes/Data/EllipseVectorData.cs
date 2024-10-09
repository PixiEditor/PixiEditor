using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.DrawingApi.Core.Surfaces.Vector;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public class EllipseVectorData : ShapeVectorData, IReadOnlyEllipseData
{
    public VecD Radius { get; set; }
    public VecD Center { get; set; }

    public override RectD GeometryAABB =>
        new ShapeCorners(Center, Radius * 2).AABBBounds;

    public override ShapeCorners TransformationCorners =>
        new ShapeCorners(Center, Radius * 2).WithMatrix(TransformationMatrix);


    public EllipseVectorData(VecD center, VecD radius)
    {
        Center = center;
        Radius = radius;
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
        int saved = drawingSurface.Canvas.Save();

        if (applyTransform)
        {
            Matrix3X3 canvasMatrix = drawingSurface.Canvas.TotalMatrix;

            Matrix3X3 final = TransformationMatrix with { TransX = 0, TransY = 0 }; 

            final = canvasMatrix.Concat(final);

            drawingSurface.Canvas.SetMatrix(final);

            paint.Color = FillColor;
            paint.Style = PaintStyle.Fill;
            drawingSurface.Canvas.DrawOval(VecD.Zero, Radius, paint);

            paint.Color = StrokeColor;
            paint.Style = PaintStyle.Stroke;
            paint.StrokeWidth = StrokeWidth;
            drawingSurface.Canvas.DrawOval(VecD.Zero, Radius - new VecD(StrokeWidth / 2f), paint);
        }

        drawingSurface.Canvas.RestoreToCount(saved);
    }

    public override bool IsValid()
    {
        return Radius is { X: > 0, Y: > 0 };
    }

    public override int CalculateHash()
    {
        return HashCode.Combine(Center, Radius, StrokeColor, FillColor, StrokeWidth, TransformationMatrix);
    }

    public override int GetCacheHash()
    {
        return CalculateHash();
    }

    public override object Clone()
    {
        return new EllipseVectorData(Center, Radius)
        {
            StrokeColor = StrokeColor,
            FillColor = FillColor,
            StrokeWidth = StrokeWidth,
            TransformationMatrix = TransformationMatrix
        };
    }
}
