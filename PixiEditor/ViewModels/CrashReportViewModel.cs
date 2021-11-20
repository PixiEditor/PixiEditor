using GalaSoft.MvvmLight.CommandWpf;
using PixiEditor.Models.DataHolders;
using System.Diagnostics;
using System.Windows;

namespace PixiEditor.ViewModels
{
    public class CrashReportViewModel : ViewModelBase
    {
        public CrashReport CrashReport { get; }

        public string ReportText { get; }

        public int DocumentCount { get; }

        public RelayCommand RecoverDocumentsCommand { get; }

        public RelayCommand AttachDebuggerCommand { get; }

        public bool IsDebugBuild { get; set; }

        public CrashReportViewModel(CrashReport report)
        {
            SetIsDebug();

            CrashReport = report;
            ReportText = report.ReportText;
            DocumentCount = report.GetDocumentCount();
            RecoverDocumentsCommand = new(RecoverDocuments);
            AttachDebuggerCommand = new(AttachDebugger);
        }

        public void RecoverDocuments()
        {
            MainWindow window = MainWindow.CreateWithDocuments(CrashReport.RecoverDocuments());

            Application.Current.MainWindow = window;
            window.Show();
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
}
