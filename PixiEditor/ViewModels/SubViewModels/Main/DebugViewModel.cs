using PixiEditor.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class DebugViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand OpenFolderCommand { get; set; }

        public RelayCommand OpenInstallLocationCommand { get; set; }

        public RelayCommand CrashCommand { get; set; }

        public DebugViewModel(ViewModelMain owner)
            : base(owner)
        {
            OpenFolderCommand = new RelayCommand(OpenFolder);
            OpenInstallLocationCommand = new RelayCommand(OpenInstallLocation);
            CrashCommand = new RelayCommand(_ => throw new InvalidOperationException("Debug Crash"));
        }

        public static void OpenFolder(object parameter)
        {
            OpenShellExecute((string)parameter);
        }

        public static void OpenInstallLocation(object parameter)
        {
            OpenShellExecute(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }

        private static void OpenShellExecute(string path)
        {
            ProcessStartInfo startInfo = new (Environment.ExpandEnvironmentVariables(path));

            startInfo.UseShellExecute = true;

            Process.Start(startInfo);
        }
    }
}