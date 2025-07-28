using PixiEditor.ViewModels;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.Views.Windows.Settings;

public partial class SettingsWindow : PixiEditorPopup
{
    public SettingsWindow(int page = 0)
    {
        InitializeComponent();
        var viewModel = DataContext as SettingsWindowViewModel;
        viewModel!.CurrentPage = page;
    }
}

