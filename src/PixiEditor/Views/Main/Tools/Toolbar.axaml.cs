using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.Views.Main.Tools;

public partial class Toolbar : UserControl
{
    public Toolbar()
    {
        InitializeComponent();
    }

    private void ToolSetItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: IToolSetHandler toolSet })
        {
            return;
        }

        ToolSetDropdownButton.Flyout?.Hide();

        Dispatcher.UIThread.Post(() =>
        {
            if (ViewModelMain.Current?.ToolsSubViewModel is ToolsViewModel toolsVm)
            {
                toolsVm.SetActiveToolSet(toolSet);
            }
        });
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
