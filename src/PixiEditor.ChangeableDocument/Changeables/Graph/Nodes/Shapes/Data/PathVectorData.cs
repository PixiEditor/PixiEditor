using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public class PathVectorData : ShapeVectorData, IReadOnlyPathData
{
    public VectorPath Path { get; }
    public override RectD GeometryAABB => Path.TightBounds;
    public override RectD VisualAABB => GeometryAABB.Inflate(StrokeWidth / 2);

    public override ShapeCorners TransformationCorners =>
        new ShapeCorners(GeometryAABB).WithMatrix(TransformationMatrix);

    public PathVectorData(VectorPath path)
    {
        Path = path;
    }

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
        int num = 0;
        if (applyTransform)
        {
            num = drawingSurface.Canvas.Save();
            ApplyTransformTo(drawingSurface);
        }

        using Paint paint = new Paint()
        {
            IsAntiAliased = true, StrokeJoin = StrokeJoin.Round, StrokeCap = StrokeCap.Round
        };

        if (FillColor.A > 0)
        {
            paint.Color = FillColor;
            paint.Style = PaintStyle.Fill;

            drawingSurface.Canvas.DrawPath(Path, paint);
        }

        paint.Color = StrokeColor;
        paint.Style = PaintStyle.Stroke;
        paint.StrokeWidth = StrokeWidth;

        drawingSurface.Canvas.DrawPath(Path, paint);

        if (applyTransform)
        {
            drawingSurface.Canvas.RestoreToCount(num);
        }
    }

    public override bool IsValid()
    {
        return Path is { IsEmpty: false };
    }

    public override int GetCacheHash()
    {
        return Path.GetHashCode();
    }

    public override int CalculateHash()
    {
        return Path.GetHashCode();
    }

    public override object Clone()
    {
        return new PathVectorData(new VectorPath(Path))
        {
            StrokeColor = StrokeColor,
            FillColor = FillColor,
            StrokeWidth = StrokeWidth,
            TransformationMatrix = TransformationMatrix
        };
    }
}
