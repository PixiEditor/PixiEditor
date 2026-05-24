using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace PixiEditor.Views.Main.Tools;

public partial class Toolbar : UserControl
{
    public Toolbar()
    {
        InitializeComponent();
    }

    private void ToolSetItem_OnClick(object? sender, RoutedEventArgs e)
        => Dispatcher.UIThread.Post(() => ToolSetDropdownButton.Flyout?.Hide());

    private void ToolSetDropdownFlyout_OnOpened(object? sender, EventArgs e)
        => ToolSetDropdownButton.Classes.Set("open", true);

    private void ToolSetDropdownFlyout_OnClosed(object? sender, EventArgs e)
        => ToolSetDropdownButton.Classes.Set("open", false);
}
