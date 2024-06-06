using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace PixiEditor.UI.Common.Rendering;

public class IconImage : IImage
{
    public string Icon { get; }
    public FontFamily FontFamily { get; }
    public double FontSize { get; }
    
    public SolidColorBrush Foreground { get; }
    public Size Size { get; }
    
    private Typeface _typeface;
    
    public IconImage(string icon, FontFamily fontFamily, double fontSize, Color foreground)
    {
        Icon = icon;
        FontFamily = fontFamily;
        FontSize = fontSize;
        Foreground = new SolidColorBrush(foreground);
        _typeface = new Typeface(FontFamily);
        Size = new Size(FontSize, FontSize);
    }
    
    public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
    {
        context.DrawText(
            new FormattedText(Icon, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, _typeface, FontSize, Foreground),
            destRect.TopLeft);
    }
}
