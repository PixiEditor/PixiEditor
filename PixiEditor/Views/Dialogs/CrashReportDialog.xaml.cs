using PixiEditor.Models.DataHolders;
using PixiEditor.ViewModels;
using System.Windows;

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
    }
}
