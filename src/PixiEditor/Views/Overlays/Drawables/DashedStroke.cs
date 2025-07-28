using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.Views.Overlays.Drawables;

public class DashedStroke
{
    private Paint blackPaint = new Paint()
    {
        Color = Colors.Black, StrokeWidth = 1, Style = PaintStyle.Stroke, IsAntiAliased = true
    };

    private Paint whiteDashPaint = new Paint()
    {
        Color = Colors.White,
        StrokeWidth = 1,
        Style = PaintStyle.Stroke,
        PathEffect = PathEffect.CreateDash(
            [2, 2], 2),
        IsAntiAliased = true
    };

    public void UpdateZoom(float newZoom)
    {
        blackPaint.StrokeWidth = (float)(1.0 / newZoom);

        whiteDashPaint.StrokeWidth = (float)(2.0 / newZoom);
        whiteDashPaint?.PathEffect?.Dispose();

        float[] dashes = [whiteDashPaint.StrokeWidth * 4, whiteDashPaint.StrokeWidth * 3];

        dashes[0] = whiteDashPaint.StrokeWidth * 4;
        dashes[1] = whiteDashPaint.StrokeWidth * 3;

        whiteDashPaint.PathEffect = PathEffect.CreateDash(dashes, 2);
    }

    public void Draw(Canvas canvas, VecD start, VecD end)
    {
        canvas.DrawLine(start, end, blackPaint);
        canvas.DrawLine(start, end, whiteDashPaint);
    }

    public void Draw(Canvas canvas, VectorPath path)
    {
        canvas.DrawPath(path, blackPaint);
        canvas.DrawPath(path, whiteDashPaint);
    }
}
