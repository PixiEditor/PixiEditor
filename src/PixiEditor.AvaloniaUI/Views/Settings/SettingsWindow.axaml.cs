using Avalonia.Controls;
using PixiEditor.AvaloniaUI.ViewModels;

namespace PixiEditor.AvaloniaUI.Views.Settings;

public partial class SettingsWindow : Window
{
    public SettingsWindow(int page = 0)
    {
        InitializeComponent();
        var viewModel = DataContext as SettingsWindowViewModel;
        viewModel!.CurrentPage = page;
    }

    //TODO figure out what's the purpose of this
    /*
    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }*/
}

