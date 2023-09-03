using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace PixiEditor.AvaloniaUI.Views.Overlays;

public class Overlay : Control
{
    protected IBrush HandleGlyphBrush { get; } = GetBrush("HandleGlyphBrush");
    protected IBrush BackgroundBrush { get; } = GetBrush("HandleBackgroundBrush");

    protected static IBrush GetBrush(string key)
    {
        if (Application.Current.Styles.TryGetResource(key, null, out object brush))
        {
            return (IBrush)brush;
        }

        return Brushes.Black;
    }

    protected static Geometry GetHandleGeometry(string handleName)
    {
        if (Application.Current.Styles.TryGetResource(handleName, null, out object shape))
        {
            return ((Path)shape).Data;
        }

        return Geometry.Parse("M 0 0 L 1 0 M 0 0 L 0 1");
    }
}
