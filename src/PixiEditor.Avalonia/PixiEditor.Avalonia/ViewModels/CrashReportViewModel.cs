using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Avalonia;
using Avalonia.Controls;
using PixiEditor.Avalonia.ViewModels;
using PixiEditor.Avalonia.Views;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Views;
using PixiEditor.Views.Dialogs;
using ReactiveUI;

namespace PixiEditor.ViewModels;

internal class CrashReportViewModel : ViewModelBase
{
    private bool hasRecoveredDocuments = true;

    public CrashReport CrashReport { get; }

    public string ReportText { get; }

    public int DocumentCount { get; }

    public ReactiveCommand<Unit, Unit> OpenSendCrashReportCommand { get; }

    public ReactiveCommand<Unit, Unit> RecoverDocumentsCommand { get; }

    public ReactiveCommand<Unit, Unit> AttachDebuggerCommand { get; }

    public bool IsDebugBuild { get; set; }

    public CrashReportViewModel(CrashReport report)
    {
        SetIsDebug();

        CrashReport = report;
        ReportText = report.ReportText;
        DocumentCount = report.GetDocumentCount();
        //TODO: Implement
        //OpenSendCrashReportCommand = ReactiveCommand.Create(() => new SendCrashReportWindow(CrashReport).Show());
        RecoverDocumentsCommand = ReactiveCommand.Create(RecoverDocuments, Observable.Create((IObserver<bool> observer) =>
        {
            observer.OnNext(hasRecoveredDocuments);
            observer.OnCompleted();
            return Disposable.Empty;
        }));

        AttachDebuggerCommand = ReactiveCommand.Create(AttachDebugger);

        if (!IsDebugBuild)
            _ = CrashHelper.SendReportTextToWebhook(report);
    }

    public void RecoverDocuments()
    {
        MainWindow window = MainWindow.CreateWithDocuments(CrashReport.RecoverDocuments());

        Application.Current.Run(window);
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
            /*TODO: MessageBox.Show("Starting debugger failed", "Starting debugger failed", MessageBoxButton.OK, MessageBoxImage.Error);*/
        }
    }
}
