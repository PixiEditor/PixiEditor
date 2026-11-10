using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;
using PixiEditor.Helpers;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays.Ruler;

public class RulerOverlay : Overlay
{
    public const double Offset = 24;
    private const double MaxDensity = 30;

    private Paint backgroundPaint;

    private static Font textFont;

    static RulerOverlay()
    {
        textFont = Font.CreateDefault();
    }

    public RulerOverlay()
    {
        backgroundPaint = ResourceLoader.GetPaint("ThemeBackgroundBrush1");
    }

    protected override void OnRenderOverlay(Canvas context, RectD canvasBounds)
    {
        int saved = context.Save();
        Matrix3X3 previousMatrix = context.TotalMatrix;

        context.SetMatrix(Matrix3X3.Identity);
        using Paint paint = new Paint();
        paint.Color = Colors.Gray;
        DrawHorizontalRuler(context, paint, previousMatrix);
        DrawVerticalRuler(context, paint, previousMatrix);
        context.RestoreToCount(saved);
    }

    private void DrawHorizontalRuler(Canvas context, Paint paint, Matrix3X3 previousMatrix)
    {
        double zeroX = previousMatrix.MapPoint(new VecD(0, 0)).X;
        double docSizePixelSpacing = previousMatrix.MapPoint(new VecD(1, 0)).X - zeroX;
        context.DrawRect(new RectD(0, 0, context.DeviceClipBounds.Width, Offset), backgroundPaint);
        context.DrawLine(new VecD(0, Offset), new VecD(context.DeviceClipBounds.Width, Offset), paint);

        double minTickSpacing = MaxDensity / docSizePixelSpacing;
        double tickSpacing = Math.Max(1, GetNiceTickSpacing(minTickSpacing));

        double startX = previousMatrix.TransX;
        double visibleStart = Offset;
        double visibleEnd = context.DeviceClipBounds.Width;

        double firstTick = Math.Ceiling((visibleStart - startX) / docSizePixelSpacing / tickSpacing) * tickSpacing;
        double lastTick = (visibleEnd - startX) / docSizePixelSpacing;

        for (double i = firstTick; i <= lastTick; i += tickSpacing)
        {
            double currentX = i * docSizePixelSpacing + startX;

            context.DrawText(i.ToString("0.##"), new VecD(currentX, textFont.Size), TextAlign.Center, textFont, paint);

            context.DrawLine(new VecD(currentX, Offset), new VecD(currentX, Offset - textFont.Size + 2), paint);
        }
    }

    private void DrawVerticalRuler(Canvas context, Paint paint, Matrix3X3 previousMatrix)
    {
        double zeroY = previousMatrix.MapPoint(new VecD(0, 0)).Y;
        double docSizePixelSpacing = previousMatrix.MapPoint(new VecD(0, 1)).Y - zeroY;
        context.DrawRect(new RectD(0, 0, Offset, context.DeviceClipBounds.Height), backgroundPaint);
        context.DrawLine(new VecD(Offset, 0), new VecD(Offset, context.DeviceClipBounds.Height), paint);

        double minTickSpacing = MaxDensity / docSizePixelSpacing;
        double tickSpacing = Math.Max(1, GetNiceTickSpacing(minTickSpacing));

        double startY = previousMatrix.TransY;
        double visibleStart = Offset;
        double visibleEnd = context.DeviceClipBounds.Width;

        double firstTick = Math.Ceiling((visibleStart - startY) / docSizePixelSpacing / tickSpacing) * tickSpacing;
        double lastTick = (visibleEnd - startY) / docSizePixelSpacing;

        for (double i = firstTick; i <= lastTick; i += tickSpacing)
        {
            double currentY = i * docSizePixelSpacing + startY;

            int save2 = context.Save();

            var textCenter = new VecD(textFont.Size, currentY);
            context.RotateDegrees(-90, (float)textCenter.X, (float)textCenter.Y);
            context.DrawText(i.ToString("0.##"), textCenter, TextAlign.Center, textFont, paint);
            context.RestoreToCount(save2);

            context.DrawLine(new VecD(Offset, currentY), new VecD(Offset - textFont.Size + 2, currentY), paint);
        }
    }

    private static double GetNiceTickSpacing(double minSpacing)
    {
        double exponent = Math.Floor(Math.Log10(minSpacing));
        double magnitude = Math.Pow(10, exponent);
        double normalized = minSpacing / magnitude;

        double nice = normalized switch
        {
            <= 1 => 1,
            <= 2 => 2,
            <= 5 => 5,
            _ => 10
        };

        return nice * magnitude;
    }
}
