using System;
using System.IO;
using System.Linq;
using PixiEditor.UpdateModule;
using ReactiveUI;

namespace PixiEditor.UpdateInstaller.New.ViewModels;

public class MainViewModel : ViewModelBase
{
    private float progressValue;

    public MainViewModel()
    {
        Current = this;

        string updateDirectory = Path.GetDirectoryName(Extensions.GetExecutablePath());

#if DEBUG
        updateDirectory = Path.GetDirectoryName(Environment.GetCommandLineArgs().FirstOrDefault());
#endif
        UpdateDirectory = updateDirectory;
    }

    public MainViewModel Current { get; private set; }

    public UpdateModule.UpdateInstaller Installer { get; set; }

    public string UpdateDirectory { get; private set; }

    public float ProgressValue
    {
        get => progressValue;
        set => this.RaiseAndSetIfChanged(ref this.progressValue, value);
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

    private void Installer_ProgressChanged(object? sender, UpdateProgressChangedEventArgs e)
    {
        ProgressValue = e.Progress;
    }
}
