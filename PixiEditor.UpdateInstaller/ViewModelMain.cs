using System;
using System.IO;
using PixiEditor.UpdateModule;

namespace PixiEditor.UpdateInstaller
{
    public class ViewModelMain : ViewModelBase
    {
        private float progressValue;

        public ViewModelMain()
        {
            Current = this;

            var updateDirectory = Path.GetDirectoryName(Extensions.GetExecutablePath());

#if DEBUG
            updateDirectory = Environment.GetCommandLineArgs()[1];
#endif
            UpdateDirectory = updateDirectory;
        }

        public ViewModelMain Current { get; }
        public UpdateModule.UpdateInstaller Installer { get; set; }

        public string UpdateDirectory { get; }

        public float ProgressValue
        {
            get => progressValue;
            set
            {
                progressValue = value;
                RaisePropertyChanged(nameof(ProgressValue));
            }
        }

        public void InstallUpdate()
        {
            var files = Directory.GetFiles(UpdateDirectory, "update-*.zip");

            if (files.Length > 0)
            {
                Installer = new UpdateModule.UpdateInstaller(files[0]);
                Installer.ProgressChanged += Installer_ProgressChanged;
                Installer.Install();
            }
            else
            {
                ProgressValue = 100;
            }
        }

        private void Installer_ProgressChanged(object sender, UpdateProgressChangedEventArgs e)
        {
            ProgressValue = e.Progress;
        }
    }
}