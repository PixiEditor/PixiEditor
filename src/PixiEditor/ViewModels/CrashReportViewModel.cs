using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
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

        if (!IsDebugBuild)
            SendReportTextToWebhook(report);
    }

    private async void SendReportTextToWebhook(CrashReport report)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(report.ReportText);
        string filename = Path.GetFileNameWithoutExtension(report.FilePath) + ".txt";

        MultipartFormDataContent formData = new MultipartFormDataContent
        {
            { new ByteArrayContent(bytes, 0, bytes.Length), "crash-report", filename }
        };
        try
        {
            using HttpClient httpClient = new HttpClient();
            string url = BuildConstants.CrashReportWebhookUrl;
            await httpClient.PostAsync(url, formData);
        }
        catch { }
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
