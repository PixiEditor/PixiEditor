using System.Diagnostics;
using System.Windows;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Views;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.ViewModels;

internal class CrashReportViewModel : ViewModelBase
{
    private bool hasRecoveredDocuments = true;

    public CrashReport CrashReport { get; }

    public string ReportText { get; }

    public int DocumentCount { get; }

    public RelayCommand OpenSendCrashReportCommand { get; }

    public RelayCommand RecoverDocumentsCommand { get; }

    public RelayCommand AttachDebuggerCommand { get; }

    public bool IsDebugBuild { get; set; }

    public CrashReportViewModel(CrashReport report)
    {
        SetIsDebug();

        CrashReport = report;
        ReportText = report.ReportText;
        DocumentCount = report.GetDocumentCount();
        OpenSendCrashReportCommand = new((_) => new SendCrashReportWindow(CrashReport).Show());
        RecoverDocumentsCommand = new(RecoverDocuments, (_) => hasRecoveredDocuments);
        AttachDebuggerCommand = new(AttachDebugger);
    }

    public void RecoverDocuments(object args)
    {
        MainWindow window = MainWindow.CreateWithDocuments(CrashReport.RecoverDocuments());

        Application.Current.MainWindow = window;
        window.Show();
        hasRecoveredDocuments = false;
    }

    [Conditional("DEBUG")]
    private void SetIsDebug()
    {
        IsDebugBuild = true;
    }

    private void AttachDebugger(object args)
    {
        if (!Debugger.Launch())
        {
            MessageBox.Show("Starting debugger failed", "Starting debugger failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
