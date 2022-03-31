using PixiEditor.Helpers;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using SkiaSharp;

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

        public void OpenFolder(object parameter)
        {
            ProcessHelpers.ShellExecuteEV(parameter as string);
        }

        public static void OpenInstallLocation(object parameter)
        {
            ProcessHelpers.ShellExecuteEV(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }
    }
}