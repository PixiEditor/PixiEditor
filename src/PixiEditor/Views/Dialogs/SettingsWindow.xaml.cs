using System.Windows;
using System.Windows.Input;
using PixiEditor.ViewModels;

namespace PixiEditor.Views.Dialogs;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
internal partial class SettingsWindow : Window
{
    public SettingsWindow(int page = 0)
    {
        InitializeComponent();
        var viewModel = DataContext as SettingsWindowViewModel;
        viewModel!.CurrentPage = page;
    }

    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }

}
