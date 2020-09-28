using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PixiEditor.UpdateInstaller
{
    public class ViewModelMain : ViewModelBase
    {
        public ViewModelMain Current { get; private set; }
        public UpdateModule.UpdateInstaller Installer { get; set; }

        private float _progressValue;

        public float ProgressValue
        {
            get => _progressValue;
            set 
            { 
                _progressValue = value;
                RaisePropertyChanged(nameof(ProgressValue));
            }
        }

        public ViewModelMain(Action closeAction)
        {
            Current = this;

            string updateDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

#if DEBUG
            updateDirectory = Environment.GetCommandLineArgs()[1];
#endif

            string[] files = Directory.GetFiles(updateDirectory, "update-*.zip");

            if (files.Length > 0)
            {
                Installer = new UpdateModule.UpdateInstaller(files[0]);
                Installer.ProgressChanged += Installer_ProgressChanged;
                Installer.Install();
            }

            string pixiEditorExecutablePath = Directory.GetFiles(updateDirectory, "PixiEditor.exe")[0];
            StartPixiEditor(pixiEditorExecutablePath);
            closeAction();
        }

        private void StartPixiEditor(string executablePath)
        {
            Process process = Process.Start(executablePath);
        }

        private void Installer_ProgressChanged(object sender, UpdateModule.UpdateProgressChangedEventArgs e)
        {
            ProgressValue = e.Progress;
        }
    }
}
