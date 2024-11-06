using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;
using PixiEditor.Helpers;

namespace PixiEditor.Views.Overlays.Drawables;

public class InfoBox
{
    private Paint fontPen = new Paint()
    {
        Color = Colors.Black, StrokeWidth = 1, Style = PaintStyle.Fill, IsAntiAliased = true
    };

    private Paint backgroundPen = new Paint()
    {
        Color = Colors.White, StrokeWidth = 1, Style = PaintStyle.Fill, IsAntiAliased = true
    };

    public double ZoomScale { get; set; } = 1;

    private Font font;

    public InfoBox()
    {
        font = ThemeResources.ThemeFont;
        fontPen.Color = ThemeResources.ForegroundColor;
        backgroundPen.Color = ThemeResources.BackgroundColor;
    }

    public void DrawInfo(Canvas context, string text, VecD pointerPos)
    {
        font.FontSize = 14 / ZoomScale;

        double widthTextSize = font.MeasureText(text);

        VecD aboveCursor = pointerPos + new VecD(0, -20 / ZoomScale);
        float rectWidth = (float)widthTextSize + (10 / (float)ZoomScale);
        float rectHeight = 20 / (float)ZoomScale;
        float x = (float)aboveCursor.X - rectWidth / 2;
        float y = (float)aboveCursor.Y - ((float)font.FontSize);
        context.DrawRoundRect(x, y, rectWidth, rectHeight, 5 / (float)ZoomScale,
            5 / (float)ZoomScale, backgroundPen);

        context.DrawText(text, aboveCursor, TextAlign.Center, font, fontPen);
    }
}
