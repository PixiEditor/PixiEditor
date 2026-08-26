using System.Windows.Input;
using Avalonia;
using Avalonia.Input;

namespace PixiEditor.Views.Nodes.Properties;

public partial class StringPropertyView : NodePropertyView
{
    public static readonly StyledProperty<ICommand> OpenInDefaultAppCommandProperty =
        AvaloniaProperty.Register<StringPropertyView, ICommand>(
            nameof(OpenInDefaultAppCommand));

    public ICommand OpenInDefaultAppCommand
    {
        get => GetValue(OpenInDefaultAppCommandProperty);
        set => SetValue(OpenInDefaultAppCommandProperty, value);
    }

    public StringPropertyView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
    }
}
