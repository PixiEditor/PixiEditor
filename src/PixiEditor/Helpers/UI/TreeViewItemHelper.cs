using Avalonia;
using Avalonia.Controls;

namespace PixiEditor.Helpers.UI;

internal static class TreeViewItemHelper
{
    public static GridLength GetIndent(AvaloniaObject obj)
    {
        return obj.GetValue(IndentProperty);
    }

    public static void SetIndent(AvaloniaObject obj, GridLength value)
    {
        obj.SetValue(IndentProperty, value);
    }

    public static readonly AttachedProperty<GridLength> IndentProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, GridLength>("Indent", typeof(TreeViewItemHelper), new GridLength(0));
}
