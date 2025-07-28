using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;

namespace PixiEditor.Views.Overlays.TextOverlay;

internal class Caret : IDisposable
{
    public int CaretPosition
    {
        get => _caretPosition;
        set
        {
            if (_caretPosition != value)
            {
                _caretPosition = value;
                visible = true;
                lastUpdate = DateTime.Now;
            }
        }
    }

    public double FontSize { get; set; }
    public VecF[] GlyphPositions { get; set; }
    public VecD Offset { get; set; }
    public float[] GlyphWidths { get; set; }
    public float CaretWidth { get; set; } = 0.5f;

    private Paint paint = new Paint() { Color = Colors.White, Style = PaintStyle.StrokeAndFill, StrokeWidth = 3 };

    private bool visible;
    private DateTime lastUpdate = DateTime.Now;
    private int _caretPosition;

    public void Render(Canvas canvas)
    {
        if (GlyphPositions.Length == 0)
        {
            return;
        }

        int clampedBlinkerPosition = Math.Clamp(CaretPosition, 0, GlyphPositions.Length - 1);

        var glyphPosition = GlyphPositions[clampedBlinkerPosition];

        var glyphHeight = FontSize;

        var x = glyphPosition.X + Offset.X;
        var y = glyphPosition.Y + Offset.Y;

        paint.StrokeWidth = CaretWidth;

        VecD from = new VecD(x, y + glyphHeight / 4f);
        VecD to = new VecD(x, y - glyphHeight);

        if (DateTime.Now - lastUpdate > TimeSpan.FromMilliseconds(500))
        {
            visible = !visible;
            lastUpdate = DateTime.Now;
        }

        paint.Color = new Color(Colors.White.R, Colors.White.G, Colors.White.B, (byte)(visible ? 255 : 0));

        VecD strokeOffset = new VecD(CaretWidth / 2, 0);
        canvas.DrawLine(from - strokeOffset, to - strokeOffset, paint);
        paint.Color = new Color(Colors.Black.R, Colors.Black.G, Colors.Black.B, (byte)(visible ? 255 : 0));
        canvas.DrawLine(from + strokeOffset, to + strokeOffset, paint);
    }

    public void Dispose()
    {
        paint.Dispose();
    }
}
