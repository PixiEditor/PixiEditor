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
}
