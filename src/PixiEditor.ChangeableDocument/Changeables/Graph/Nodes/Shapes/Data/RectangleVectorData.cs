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
    public VecD Center { get; set; }
    public VecD Size { get; set; }
    public double CornerRadius { get; set; }

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
        new ShapeCorners(GeometryAABB).WithMatrix(TransformationMatrix);


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
            DrawRect(canvas, paint);
        }

        if (StrokeWidth > 0 && Stroke.AnythingVisible)
        {
            paint.SetPaintable(Stroke);
            paint.Style = PaintStyle.Stroke;

            paint.StrokeWidth = StrokeWidth;

            DrawRect(canvas, paint);
        }

        if (applyTransform)
        {
            canvas.RestoreToCount(saved);
        }
    }

    private void DrawRect(Canvas canvas, Paint paint)
    {
        double maxRadiusPx = Math.Min(Size.X, Size.Y) / 2f;
        double radiusPx = CornerRadius * maxRadiusPx;

        if (radiusPx == 0)
        {
            canvas.DrawRect(RectD.FromCenterAndSize(Center, Size), paint);
        }
        else
        {
            RectD rect = RectD.FromCenterAndSize(Center, Size);
            canvas.DrawRoundRect((float)rect.Pos.X, (float)rect.Pos.Y, (float)rect.Width, (float)rect.Height, (float)radiusPx, (float)radiusPx, paint);
        }
    }

    public override bool IsValid()
    {
        return Size is { X: > 0, Y: > 0 };
    }

    protected override int GetSpecificHash()
    {
        return HashCode.Combine(Center, Size, CornerRadius);
    }

    public override VectorPath ToPath(bool transformed = false)
    {
        VectorPath path = new VectorPath();
        if (CornerRadius == 0)
        {
            path.AddRect(RectD.FromCenterAndSize(Center, Size));
        }
        else
        {
            double maxRadiusPx = Math.Min(Size.X, Size.Y) / 2f;
            double radiusPx = CornerRadius * maxRadiusPx;
            path.AddRoundRect(RectD.FromCenterAndSize(Center, Size), new VecD(radiusPx));
        }

        if (transformed)
        {
            path.Transform(TransformationMatrix);
        }

        return path;
    }

    protected bool Equals(RectangleVectorData other)
    {
        return base.Equals(other) && Center.Equals(other.Center) && Size.Equals(other.Size) &&
               CornerRadius.Equals(other.CornerRadius);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((RectangleVectorData)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Center, Size, CornerRadius);
    }
}
