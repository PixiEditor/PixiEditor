using Avalonia.Controls;
using PixiEditor.AvaloniaUI.Models.ExceptionHandling;
using CrashReportViewModel = PixiEditor.AvaloniaUI.ViewModels.CrashReportViewModel;

namespace PixiEditor.AvaloniaUI.Views.Dialogs;

/// <summary>
/// Interaction logic for CrashReportDialog.xaml
/// </summary>
internal partial class CrashReportDialog : Window
{
    public CrashReportDialog(CrashReport report)
    {
        DataContext = new CrashReportViewModel(report);
        InitializeComponent();
    }

    /*private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }*/
}
