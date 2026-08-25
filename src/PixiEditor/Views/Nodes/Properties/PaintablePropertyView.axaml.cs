using Avalonia.Input;

namespace PixiEditor.Views.Nodes.Properties;

public partial class PaintablePropertyView : NodePropertyView
{
    public PaintablePropertyView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
    }
}

