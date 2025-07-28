using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.IO;
using PixiEditor.OperatingSystem;
using PixiEditor.ViewModels.Nodes.Properties;

namespace PixiEditor.Views.Nodes.Properties;

public partial class StringPropertyView : NodePropertyView
{
    public static readonly StyledProperty<ICommand> OpenInDefaultAppCommandProperty = AvaloniaProperty.Register<StringPropertyView, ICommand>(
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

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ScrollViewer scroll = smallTextBox.FindDescendantOfType<ScrollViewer>();
        scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
        scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
    }

    private void InputElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        e.Handled = true;
    }

    private void Popup_OnOpened(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() => bigTextBox.Focus());
    }
}
