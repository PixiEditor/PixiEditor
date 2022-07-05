using System.Diagnostics;
using System.Windows;
using GalaSoft.MvvmLight.CommandWpf;
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
        OpenSendCrashReportCommand = new(() => new SendCrashReportWindow(CrashReport).Show());
        RecoverDocumentsCommand = new(RecoverDocuments, () => hasRecoveredDocuments, false);
        AttachDebuggerCommand = new(AttachDebugger);
    }

    public void RecoverDocuments()
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

    private void AttachDebugger()
    {
        if (!Debugger.Launch())
        {
            MessageBox.Show("Starting debugger failed", "Starting debugger failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
