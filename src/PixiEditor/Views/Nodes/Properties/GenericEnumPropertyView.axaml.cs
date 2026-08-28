using Avalonia.Input;

namespace PixiEditor.Views.Nodes.Properties;

public partial class GenericEnumPropertyView : NodePropertyView
{
    public GenericEnumPropertyView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
    }
}
