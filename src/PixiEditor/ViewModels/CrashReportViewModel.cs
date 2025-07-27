using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Helpers;
using PixiEditor.Initialization;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.ExceptionHandling;
using PixiEditor.Platform;
using PixiEditor.UI.Common.Localization;
using PixiEditor.Views;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.ViewModels;

internal partial class CrashReportViewModel : Window
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

        _ = CrashHelper.SendReportToAnalyticsApiAsync(report);
    }

    [RelayCommand(CanExecute = nameof(CanRecoverDocuments))]
    public async Task RecoverDocuments()
    {
        if (!hasRecoveredDocuments)
        {
            return;
        }

        MainWindow window = MainWindow.CreateWithRecoveredDocuments(CrashReport, out var showMissingFilesDialog);

        window.Loaded += (sender, args) =>
        {
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

                _ = dialog.ShowDialog(true);
            }
        };

        hasRecoveredDocuments = false;
        Application.Current.Run(window);
    }

    [RelayCommand]
    public void RunInSafeMode()
    {
        (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
        Process.Start(
            new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty,
                Arguments = "--safeMode",
                UseShellExecute = true
            });
    }

    public bool CanRecoverDocuments()
    {
        return hasRecoveredDocuments;
    }

    public static void ShowMissingFilesDialog(CrashReport crashReport)
    {
        var dialog = new OptionsDialog<LocalizedString>(
            "CRASH_NOT_ALL_DOCUMENTS_RECOVERED_TITLE",
            new LocalizedString("CRASH_NOT_ALL_DOCUMENTS_RECOVERED"), MainWindow.Current)
        {
            {
                "SEND", _ =>
                {
                    var sendReportDialog = new SendCrashReportDialog(crashReport);
                    sendReportDialog.ShowDialog(MainWindow.Current);
                }
            },
            "CLOSE"
        };

        dialog.ShowDialog(true);
    }


    [Conditional("DEBUG")]
    private void SetIsDebug()
    {
        IsDebugBuild = true;
    }
}
