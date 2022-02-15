using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels;
using PixiEditor.Views.Dialogs;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace PixiEditor
{
    /// <summary>
    ///     Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            string arguments = string.Join(' ', e.Args);

            if (ParseArgument("--crash (\"?)([A-z0-9:\\/\\ -_.]+)\\1", arguments, out Group[] groups))
            {
                CrashReport report = CrashReport.Parse(groups[2].Value);
                MainWindow = new CrashReportDialog(report);
            }
            else
            {
                MainWindow = new MainWindow();
            }

            MainWindow.Show();
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            base.OnSessionEnding(e);

            if (ViewModelMain.Current.BitmapManager.Documents.Any(x => !x.ChangesSaved))
            {
                ConfirmationType confirmation = ConfirmationDialog.Show($"{e.ReasonSessionEnding} with unsaved data. Are you sure?", $"{e.ReasonSessionEnding}");
                e.Cancel = confirmation != ConfirmationType.Yes;
            }
        }

        private bool ParseArgument(string pattern, string args, out Group[] groups)
        {
            Match match = Regex.Match(args, pattern, RegexOptions.IgnoreCase);
            groups = null;

            if (match.Success)
            {
                groups = match.Groups.Values.ToArray();
            }

            return match.Success;
        }
    }
}
