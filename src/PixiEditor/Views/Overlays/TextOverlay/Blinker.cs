using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;

namespace PixiEditor.Views.Overlays.TextOverlay;

internal class Blinker : IDisposable
{
    public int BlinkerPosition { get; set; }
    public double FontSize { get; set; }
    public VecF[] GlyphPositions { get; set; }
    public VecD Offset { get; set; }
    public float[] GlyphWidths { get; set; }
    public float BlinkerWidth { get; set; } = 1;

    private Paint paint = new Paint() { Color = Colors.White, Style = PaintStyle.StrokeAndFill, StrokeWidth = 1 };

    public void Render(Canvas canvas)
    {
        if (GlyphPositions.Length == 0)
        {
            return;
        }

        int clampedBlinkerPosition = Math.Clamp(BlinkerPosition, 0, GlyphPositions.Length - 1);

        var glyphPosition = GlyphPositions[clampedBlinkerPosition];

        var glyphHeight = FontSize;

        var x = glyphPosition.X + Offset.X;
        var y = glyphPosition.Y + Offset.Y;
        
        paint.StrokeWidth = BlinkerWidth;

        VecD from = new VecD(x, y);
        VecD to = new VecD(x, y - glyphHeight);
        canvas.DrawLine(from, to, paint);
    }

    public void Dispose()
    {
        paint.Dispose();
    }
}
