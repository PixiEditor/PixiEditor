using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public class TextVectorData : ShapeVectorData
{
    public string Text { get; set; }
    public VecD Position { get; set; }
    public Font Font { get; set; }

    public override RectD GeometryAABB => new RectD(Position.X, Position.Y, Font.MeasureText(Text), Font.Size);

    public override ShapeCorners TransformationCorners =>
        new ShapeCorners(GeometryAABB).WithMatrix(TransformationMatrix);

    public override RectD VisualAABB => GeometryAABB;
    public VectorPath? Path { get; set; }

    public override VectorPath ToPath()
    {
        return Font.GetTextPath(Text);
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
        int num = 0;
        if (applyTransform)
        {
            num = canvas.Save();
            ApplyTransformTo(canvas);
        }

        using Paint paint = new Paint() { IsAntiAliased = true, };

        if (Fill && FillColor.A > 0)
        {
            paint.Color = FillColor;
            paint.Style = PaintStyle.Fill;

            if (Path == null)
            {
                canvas.DrawText(Text, Position, Font, paint);
            }
            else
            {
                canvas.DrawTextOnPath(Path, Text, Position, Font, paint);
            }
        }

        if (StrokeWidth > 0)
        {
            paint.Color = StrokeColor;
            paint.Style = PaintStyle.Stroke;
            paint.StrokeWidth = StrokeWidth;

            if (Path == null)
            {
                canvas.DrawText(Text, Position, Font, paint);
            }
            else
            {
                canvas.DrawTextOnPath(Path, Text, Position, Font, paint);
            }
        }

        if (applyTransform)
        {
            canvas.RestoreToCount(num);
        }
    }

    public override bool IsValid()
    {
        return !string.IsNullOrEmpty(Text);
    }

    public override int GetCacheHash()
    {
        return HashCode.Combine(Text, Position, Font, StrokeColor, FillColor, StrokeWidth, TransformationMatrix);
    }

    public override int CalculateHash()
    {
        return GetCacheHash();
    }
}
