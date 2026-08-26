using Avalonia.Input;

namespace PixiEditor.Views.Nodes.Properties;

public partial class ColorPropertyView : NodePropertyView
{
    public ColorPropertyView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
    }
}

