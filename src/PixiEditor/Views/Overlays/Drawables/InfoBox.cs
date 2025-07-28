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
    
    private Paint borderPen = new Paint()
    {
        Color = Colors.Black, StrokeWidth = 1, Style = PaintStyle.Stroke, IsAntiAliased = true
    };

    public double ZoomScale { get; set; } = 1;

    private Font font;

    public InfoBox()
    {
        font = ThemeResources.ThemeFont;
        fontPen.Color = ThemeResources.ForegroundColor;
        backgroundPen.Color = ThemeResources.BackgroundColor;
        borderPen.Color = ThemeResources.BorderMidColor;
    }

    public void DrawInfo(Canvas context, string text, VecD pointerPos)
    {
        const float padding = 10;
        font.Size = 14 / ZoomScale;

        double widthTextSize = font.MeasureText(text);

        VecD aboveCursor = pointerPos + new VecD(0, -20 / ZoomScale);
        float rectWidth = (float)widthTextSize + (padding * 2 / (float)ZoomScale);
        float rectHeight = (float)font.Size + padding / (float)ZoomScale;
        float x = (float)aboveCursor.X - rectWidth / 2;
        float y = (float)aboveCursor.Y - ((float)font.Size) - (padding / 4) / (float)ZoomScale;
        
        context.DrawRoundRect(x, y, rectWidth, rectHeight, 5 / (float)ZoomScale,
            5 / (float)ZoomScale, backgroundPen);
        
        borderPen.StrokeWidth = 1 / (float)ZoomScale;
        
        context.DrawRoundRect(x, y, rectWidth, rectHeight, 5 / (float)ZoomScale,
            5 / (float)ZoomScale, borderPen);

        context.DrawText(text, aboveCursor, TextAlign.Center, font, fontPen);
    }
}
