using PixiEditor.UpdateModule;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PixiEditor.UpdateInstaller
{
    public class ViewModelMain : ViewModelBase
    {
        public ViewModelMain Current { get; private set; }
        public UpdateModule.UpdateInstaller Installer { get; set; }

        public string UpdateDirectory { get; private set; }

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

        public ViewModelMain()
        {
            Current = this;

            string updateDirectory = Path.GetDirectoryName(Extensions.GetExecutablePath());

#if DEBUG
            updateDirectory = Environment.GetCommandLineArgs()[1];
#endif
            UpdateDirectory = updateDirectory;
        }

        public void InstallUpdate()
        {
            string[] files = Directory.GetFiles(UpdateDownloader.DownloadLocation, "update-*.zip");

            if (files.Length > 0)
            {
                Installer = new UpdateModule.UpdateInstaller(files[0], UpdateDirectory);
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
