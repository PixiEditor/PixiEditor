using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace PixiEditor.Views.Nodes.Properties;

public partial class GenericEnumPropertyView : NodePropertyView
{
    public GenericEnumPropertyView()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        HideSocket(true, false);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
    }
}
