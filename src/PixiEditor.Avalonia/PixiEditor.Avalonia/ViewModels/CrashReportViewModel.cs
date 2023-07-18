using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Avalonia.ViewModels;
using PixiEditor.Avalonia.Views;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.ViewModels;

internal partial class CrashReportViewModel : ViewModelBase
{
    private bool hasRecoveredDocuments = true;

    public CrashReport CrashReport { get; }

    public string ReportText { get; }

    public int DocumentCount { get; }

    public bool IsDebugBuild { get; set; }

    public CrashReportViewModel(CrashReport report)
    {
        SetIsDebug();

        CrashReport = report;
        ReportText = report.ReportText;
        DocumentCount = report.GetDocumentCount();
        //TODO: Implement
        //OpenSendCrashReportCommand = ReactiveCommand.Create(() => new SendCrashReportWindow(CrashReport).Show());

        if (!IsDebugBuild)
            _ = CrashHelper.SendReportTextToWebhook(report);
    }

    [RelayCommand(CanExecute = nameof(CanRecoverDocuments))]
    public void RecoverDocuments()
    {
        MainWindow window = MainWindow.CreateWithDocuments(CrashReport.RecoverDocuments());

        Application.Current.Run(window);
        window.Show();
        hasRecoveredDocuments = false;
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
