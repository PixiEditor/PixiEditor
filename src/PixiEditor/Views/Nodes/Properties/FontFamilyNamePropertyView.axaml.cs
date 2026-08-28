using Avalonia.Input;

namespace PixiEditor.Views.Nodes.Properties;

public partial class FontFamilyNamePropertyView : NodePropertyView
{
    public FontFamilyNamePropertyView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
    }
}
