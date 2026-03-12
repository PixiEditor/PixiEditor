using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

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
