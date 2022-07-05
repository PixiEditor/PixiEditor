using System.Windows;
using System.Windows.Input;
using PixiEditor.Models.DataHolders;
using PixiEditor.ViewModels;

namespace PixiEditor.Views.Dialogs;

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

    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }
}
