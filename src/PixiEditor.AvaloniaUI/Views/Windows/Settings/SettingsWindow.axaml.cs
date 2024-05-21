using PixiEditor.AvaloniaUI.ViewModels;
using PixiEditor.AvaloniaUI.Views.Dialogs;

namespace PixiEditor.AvaloniaUI.Views.Windows.Settings;

public partial class SettingsWindow : PixiEditorPopup
{
    public SettingsWindow(int page = 0)
    {
        InitializeComponent();
        var viewModel = DataContext as SettingsWindowViewModel;
        viewModel!.CurrentPage = page;
    }
}

