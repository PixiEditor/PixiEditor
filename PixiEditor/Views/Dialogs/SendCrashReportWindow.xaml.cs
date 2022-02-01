using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PixiEditor.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for SendCrashReportWindow.xaml
    /// </summary>
    public partial class SendCrashReportWindow : Window
    {
        private readonly CrashReport report;

        public SendCrashReportWindow(CrashReport report)
        {
            this.report = report;
            InitializeComponent();
        }

        private void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.SetFileDropList(new() { report.FilePath });
        }

        private void OpenInExplorer(object sender, RoutedEventArgs e)
        {
            ProcessHelpers.ShellExecute(report.FilePath);
        }

        private void OpenHyperlink(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            ProcessHelpers.ShellExecute(button.Tag as string);
        }
    }
}
