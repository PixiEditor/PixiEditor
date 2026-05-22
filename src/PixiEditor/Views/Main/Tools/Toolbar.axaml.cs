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
    {
        Dispatcher.UIThread.Post(() => ToolSetDropdownButton.Flyout?.Hide());
    }

    private void ToolSetDropdownFlyout_OnOpened(object? sender, EventArgs e)
    {
        SetToolSetDropdownOpen(true);
    }

    private void ToolSetDropdownFlyout_OnClosed(object? sender, EventArgs e)
    {
        SetToolSetDropdownOpen(false);
    }

    private void SetToolSetDropdownOpen(bool isOpen)
    {
        const string openClass = "open";

        if (isOpen)
        {
            if (!ToolSetDropdownButton.Classes.Contains(openClass))
            {
                ToolSetDropdownButton.Classes.Add(openClass);
            }

            return;
        }

        ToolSetDropdownButton.Classes.Remove(openClass);
    }
}
