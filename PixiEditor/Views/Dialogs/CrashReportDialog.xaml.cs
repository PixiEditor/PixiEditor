using PixiEditor.Models.DataHolders;
using PixiEditor.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for CrashReportDialog.xaml
    /// </summary>
    public partial class CrashReportDialog : Window
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
}
