using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
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

        if (!IsDebugBuild)
            _ = CrashHelper.SendReportTextToWebhookAsync(report);
    }

    public void RecoverDocuments(object args)
    {
        MainWindow window = MainWindow.CreateWithRecoveredDocuments(CrashReport, out var showMissingFilesDialog);

        Application.Current.MainWindow = window;
        window.Show();
        hasRecoveredDocuments = false;

        if (showMissingFilesDialog)
        {
            var dialog = new OptionsDialog<LocalizedString>(
                "CRASH_NOT_ALL_DOCUMENTS_RECOVERED_TITLE",
                new LocalizedString("CRASH_NOT_ALL_DOCUMENTS_RECOVERED"))
            {
                {
                    "SEND", _ =>
                    {
                        var sendReportDialog = new SendCrashReportWindow(CrashReport);
                        sendReportDialog.ShowDialog();
                    }
                },
                "CLOSE"
            };

            dialog.ShowDialog(true);
        }
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
