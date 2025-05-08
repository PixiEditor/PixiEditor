using Avalonia;
using Avalonia.Controls;

namespace PixiEditor.Extensions.UI.Overlays;

public class ExpandedDecorator
{
    public static readonly AttachedProperty<int> FlexProperty =
        AvaloniaProperty.RegisterAttached<ExpandedDecorator, Control, int>("Flex");

    public static void SetFlex(Control obj, int value) => obj.SetValue(FlexProperty, value);
    public static int GetFlex(Control obj) => obj.GetValue(FlexProperty);
}
