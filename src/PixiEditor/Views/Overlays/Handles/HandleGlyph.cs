using Avalonia;
using Avalonia.Media;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;
using PixiEditor.UI.Common.Fonts;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

namespace PixiEditor.Views.Overlays.Handles;

public abstract class HandleGlyph
{
    public VecD Size { get; set; }

    public VecD Offset { get; set; }

    public HandleGlyph()
    {
    }

    public void Draw(Canvas context, double zoomboxScale, VecD position)
    {
        VecD scale = NormalizeGlyph(zoomboxScale);
        VecD offset = CalculateOffset(zoomboxScale, position);

        int saved = context.Save();

        context.Translate((float)offset.X, (float)offset.Y);
        context.Scale((float)scale.X, (float)scale.Y);

        DrawHandle(context);

        context.RestoreToCount(saved);
    }
    
    protected abstract void DrawHandle(Canvas context);
    protected abstract RectD GetBounds();

    private VecD CalculateOffset(double zoomboxScale, VecD position)
    {
        RectD bounds = GetBounds();
        VecD scaledPosition = position + new VecD(-Size.X / 2f / zoomboxScale, Size.Y / 2f / zoomboxScale);
        VecD scaledOffset = Offset / zoomboxScale;

        return new VecD(scaledPosition.X, scaledPosition.Y) + scaledOffset;
    }

    private VecD NormalizeGlyph(double scale)
    {
        RectD bounds = GetBounds();
        double scaleX = Size.X / bounds.Width / scale;
        double scaleY = Size.Y / bounds.Height / scale;

        return new VecD(scaleX, scaleY);
    }
}
