using Avalonia.Controls;
using PixiEditor.Models.ExceptionHandling;
using CrashReportViewModel = PixiEditor.ViewModels.CrashReportViewModel;
using ViewModels_CrashReportViewModel = PixiEditor.ViewModels.CrashReportViewModel;

namespace PixiEditor.Views.Dialogs;

/// <summary>
/// Interaction logic for CrashReportDialog.xaml
/// </summary>
internal partial class CrashReportDialog : Window
{
    public CrashReportDialog(CrashReport report)
    {
        DataContext = new ViewModels_CrashReportViewModel(report);
        InitializeComponent();
    }
}
