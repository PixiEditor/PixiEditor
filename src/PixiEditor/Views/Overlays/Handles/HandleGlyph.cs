using Avalonia;
using Avalonia.Media;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.Views.Overlays.Handles;

public class HandleGlyph
{
    public DrawingGroup Glyph { get; set; }

    public VecD Size { get; set; }

    private Rect originalBounds;

    public HandleGlyph(DrawingGroup glyph)
    {
        Glyph = glyph;
        originalBounds = glyph.GetBounds();
    }

    public void Draw(DrawingContext context, double zoomboxScale, VecD position)
    {
        VecD scale = NormalizeGlyph(zoomboxScale);
        VecD offset = CalculateOffset(zoomboxScale, position);

        Glyph.Transform = new MatrixTransform(
            new Matrix(
                scale.X, 0,
                0, scale.Y,
                offset.X, offset.Y)
        );
        Glyph.Draw(context);
    }

    private VecD CalculateOffset(double zoomboxScale, VecD position)
    {
        return new VecD(position.X - Size.X / (zoomboxScale * 2) - originalBounds.Position.X / (zoomboxScale * 2), position.Y - Size.Y / (zoomboxScale * 2) - originalBounds.Position.Y / (zoomboxScale * 2));
    }

    private VecD NormalizeGlyph(double scale)
    {
        double scaleX = Size.X / originalBounds.Width / scale;
        double scaleY = Size.Y / originalBounds.Height / scale;

        return new VecD(scaleX, scaleY);
    }
}
