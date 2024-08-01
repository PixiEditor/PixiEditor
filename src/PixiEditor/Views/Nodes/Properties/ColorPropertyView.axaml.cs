using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

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

