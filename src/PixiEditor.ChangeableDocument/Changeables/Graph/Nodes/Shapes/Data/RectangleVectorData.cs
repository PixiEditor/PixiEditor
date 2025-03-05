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

    public override void RasterizeGeometry(Canvas canvas)
    {
        Rasterize(canvas, false);
    }

    public override void RasterizeTransformed(Canvas canvas)
    {
        Rasterize(canvas, true);
    }

    private void Rasterize(Canvas canvas, bool applyTransform)
    {
        int saved = 0;
        if (applyTransform)
        {
            saved = canvas.Save();
            ApplyTransformTo(canvas);
        }

        using Paint paint = new Paint();
        paint.IsAntiAliased = true;

        if (Fill && FillPaintable.AnythingVisible)
        {
            paint.SetPaintable(FillPaintable);
            paint.Style = PaintStyle.Fill;
            canvas.DrawRect(RectD.FromCenterAndSize(Center, Size), paint);
        }

        if (StrokeWidth > 0 && Stroke.AnythingVisible)
        {
            paint.SetPaintable(Stroke);
            paint.Style = PaintStyle.Stroke;

            paint.StrokeWidth = StrokeWidth;
            canvas.DrawRect(RectD.FromCenterAndSize(Center, Size), paint);
        }

        if (applyTransform)
        {
            canvas.RestoreToCount(saved);
        }
    }

    public override bool IsValid()
    {
        return Size is { X: > 0, Y: > 0 };
    }

    protected override int GetSpecificHash()
    {
        return HashCode.Combine(Center, Size);
    }

    public override VectorPath ToPath()
    {
        VectorPath path = new VectorPath();
        path.AddRect(RectD.FromCenterAndSize(Center, Size));
        return path;
    }
}
