using Avalonia;
using Avalonia.Controls;

namespace PixiEditor.UI.Common.Controls.Dock.Controls;

/// <summary>
///     StretchableStackPanel resizes all children equally to fit the width of the panel if panel is smaller than desirable children size.
/// </summary>
public class StretchableStackPanel : StackPanel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);
        if (size.Width > availableSize.Width)
        {
            var width = availableSize.Width / Children.Count;
            foreach (var child in Children)
            {
                child.Measure(new Size(width, availableSize.Height));
            }
        }

        return size;
    }
}
