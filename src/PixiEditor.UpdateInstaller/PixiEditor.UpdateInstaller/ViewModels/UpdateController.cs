using System;
using System.IO;
using System.Linq;
using System.Text;
using PixiEditor.UpdateModule;

namespace PixiEditor.UpdateInstaller.ViewModels;

public class UpdateController
{
    public UpdateController()
    {
        Current = this;

        string updateDirectory = Path.GetDirectoryName(Extensions.GetExecutablePath());
        if (OperatingSystem.IsMacOS())
        {
            updateDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        }
        else
        {
            string infoPath = Path.Join(Path.GetTempPath(), "PixiEditor", "update-location.txt");
            if (File.Exists(infoPath))
            {
                try
                {
                    updateDirectory = File.ReadAllText(infoPath);
                }
                catch (Exception ex)
                {
                }
            }
        }

        UpdateDirectory = updateDirectory;
    }

    public UpdateController Current { get; private set; }

    public UpdateModule.UpdateInstaller Installer { get; set; }

    public string UpdateDirectory { get; private set; }


    public void InstallUpdate(StringBuilder log)
    {
        string[] files = Directory.GetFiles(UpdateDownloader.DownloadLocation, $"update-*.zip");
        log.AppendLine($"Found {files.Length} update files.");

        if (files.Length > 0)
        {
            Installer = new UpdateModule.UpdateInstaller(files[0], UpdateDirectory);
            log.AppendLine($"Installing update from {files[0]} to {UpdateDirectory}");
            Installer.Install(log);
        }
    }
}
