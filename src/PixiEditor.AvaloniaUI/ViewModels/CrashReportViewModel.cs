using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Dialogs;
using PixiEditor.AvaloniaUI.Models.ExceptionHandling;
using PixiEditor.AvaloniaUI.Views;
using PixiEditor.AvaloniaUI.Views.Dialogs;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.ViewModels;

internal partial class CrashReportViewModel : ViewModelBase
{
    private bool hasRecoveredDocuments = true;

    public CrashReport CrashReport { get; }

    public string ReportText { get; }

    public int DocumentCount { get; }

    public bool IsDebugBuild { get; set; }

    public RelayCommand OpenSendCrashReportCommand { get; }

    public CrashReportViewModel(CrashReport report)
    {
        SetIsDebug();

        CrashReport = report;
        ReportText = report.ReportText;
        DocumentCount = report.GetDocumentCount();
        OpenSendCrashReportCommand = new RelayCommand(() => new SendCrashReportDialog(CrashReport).Show());

        if (!IsDebugBuild)
            _ = CrashHelper.SendReportTextToWebhookAsync(report);
    }

    [RelayCommand(CanExecute = nameof(CanRecoverDocuments))]
    public async Task RecoverDocuments()
    {
        MainWindow window = MainWindow.CreateWithRecoveredDocuments(CrashReport, out var showMissingFilesDialog);

        Application.Current.Run(window);
        window.Show();
        hasRecoveredDocuments = false;
        
        if (showMissingFilesDialog)
        {
            var dialog = new OptionsDialog<LocalizedString>(
                "CRASH_NOT_ALL_DOCUMENTS_RECOVERED_TITLE",
                new LocalizedString("CRASH_NOT_ALL_DOCUMENTS_RECOVERED"), 
                MainWindow.Current!)
            {
                {
                    "SEND", _ =>
                    {
                        var sendReportDialog = new SendCrashReportDialog(CrashReport);
                        sendReportDialog.ShowDialog(window);
                    }
                },
                "CLOSE"
            };

            await dialog.ShowDialog(true);
        }
    }

    public bool CanRecoverDocuments()
    {
        return hasRecoveredDocuments;
    }

    [Conditional("DEBUG")]
    private void SetIsDebug()
    {
        IsDebugBuild = true;
    }

    [RelayCommand]
    private void AttachDebugger()
    {
        if (!Debugger.Launch())
        {
            /*TODO: MessageBox.Show("Starting debugger failed", "Starting debugger failed", MessageBoxButton.OK, MessageBoxImage.Error);*/
        }
    }
}
