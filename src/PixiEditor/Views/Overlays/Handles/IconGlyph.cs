using Avalonia.Media;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;
using PixiEditor.UI.Common.Fonts;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

namespace PixiEditor.Views.Overlays.Handles;

public class IconGlyph : HandleGlyph
{
    public string Icon { get; set; }
    
    private Paint fontPaint = new() { Color = Colors.White, IsAntiAliased = true };
    private Font targetFont;
    
    private static Font? pixiPerfectFont;

    public IconGlyph(string icon, Font font = null, Paint customPaint = null)
    {
        Icon = icon;
        pixiPerfectFont ??= Font.FromStream(PixiPerfectIconExtensions.GetFontStream());
        targetFont = font ?? pixiPerfectFont;
        if (customPaint != null)
        {
            fontPaint?.Dispose();
            fontPaint = customPaint;
        }
    }

    protected override void DrawHandle(Canvas context)
    {
        context.DrawText(Icon, VecD.Zero, targetFont, fontPaint);
    }

    protected override RectD GetBounds()
    {
        double measure = targetFont.MeasureText(Icon);
        return new RectD(0, 0, measure, targetFont.Size);
    }
}
