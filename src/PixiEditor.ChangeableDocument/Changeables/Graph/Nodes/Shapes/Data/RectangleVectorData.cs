using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public class RectangleVectorData : ShapeVectorData, IReadOnlyRectangleData
{
    public VecD Center { get; }
    public VecD Size { get; }

    public override RectD GeometryAABB => RectD.FromCenterAndSize(Center, Size);

    public override RectD VisualAABB
    {
        get
        {
            RectD bounds = RectD.FromCenterAndSize(Center, Size);
            bounds = bounds.Inflate(StrokeWidth / 2);
            return bounds;
        }
    }

    public override ShapeCorners TransformationCorners =>
        new ShapeCorners(Center, Size).WithMatrix(TransformationMatrix);


    public RectangleVectorData(VecD center, VecD size)
    {
        Center = center;
        Size = size;
    }
    
    public RectangleVectorData(double x, double y, double width, double height)
    {
        Center = new VecD(x + width / 2, y + height / 2);
        Size = new VecD(width, height);
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
        int saved = 0;
        if (applyTransform)
        {
            saved = drawingSurface.Canvas.Save();
            ApplyTransformTo(drawingSurface);
        }

        using Paint paint = new Paint();
        paint.IsAntiAliased = true;

        if (Fill && FillColor.A > 0)
        {
            paint.Color = FillColor;
            paint.Style = PaintStyle.Fill;
            drawingSurface.Canvas.DrawRect(RectD.FromCenterAndSize(Center, Size), paint);
        }

        if (StrokeWidth > 0)
        {
            paint.Color = StrokeColor;
            paint.Style = PaintStyle.Stroke;

            paint.StrokeWidth = StrokeWidth;
            drawingSurface.Canvas.DrawRect(RectD.FromCenterAndSize(Center, Size), paint);
        }

        if (applyTransform)
        {
            drawingSurface.Canvas.RestoreToCount(saved);
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

    public override VectorPath ToPath()
    {
        VectorPath path = new VectorPath();
        path.AddRect(RectD.FromCenterAndSize(Center, Size));
        return path;
    }
}
