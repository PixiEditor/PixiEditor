using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace PixiEditor.UpdateModule;

public class UpdateInstaller
{
    public const string TargetDirectoryName = "UpdateFiles";

    private float progress = 0;

    public UpdateInstaller(string archiveFileName, string targetDirectory)
    {
        ArchiveFileName = archiveFileName;
        TargetDirectory = targetDirectory;
    }

    public event EventHandler<UpdateProgressChangedEventArgs> ProgressChanged;

    public static string UpdateFilesPath { get; set; } = Path.Join(UpdateDownloader.DownloadLocation, TargetDirectoryName);

    public float Progress
    {
        get => progress;
        set
        {
            progress = value;
            ProgressChanged?.Invoke(this, new UpdateProgressChangedEventArgs(value));
        }
    }

    public string ArchiveFileName { get; set; }

    public string TargetDirectory { get; set; }

    public void Install()
    {
        var processes = Process.GetProcessesByName("PixiEditor");
        if (processes.Length > 0)
        {
            processes[0].WaitForExit();
        }

        ZipFile.ExtractToDirectory(ArchiveFileName, UpdateFilesPath, true);
        Progress = 25; // 25% for unzip
        string dirWithFiles = Directory.GetDirectories(UpdateFilesPath)[0];
        CopyFilesToDestination(dirWithFiles);
        DeleteArchive();
        Progress = 100;
    }

    private void DeleteArchive()
    {
        File.Delete(ArchiveFileName);
        Directory.Delete(UpdateFilesPath, true);
    }

    private void CopyFilesToDestination(string sourceDirectory)
    {
        int totalFiles = Directory.GetFiles(UpdateFilesPath, "*", SearchOption.AllDirectories).Length;

        string[] files = Directory.GetFiles(sourceDirectory);
        float fileCopiedVal = 74f / totalFiles; // 74% is reserved for copying

        foreach (string file in files)
        {
            string targetFileName = Path.GetFileName(file);
            File.Copy(file, Path.Join(TargetDirectory, targetFileName), true);
            Progress += fileCopiedVal;
        }

        CopySubDirectories(sourceDirectory, TargetDirectory, fileCopiedVal);
    }

    private void CopySubDirectories(string originDirectory, string targetDirectory, float percentPerFile)
    {
        string[] subDirs = Directory.GetDirectories(originDirectory);
        if(subDirs.Length == 0) return;

        foreach (string subDir in subDirs)
        {
            string targetDirPath = Path.Join(targetDirectory, Path.GetFileName(subDir));

            CopySubDirectories(subDir, targetDirPath, percentPerFile);

            string[] files = Directory.GetFiles(subDir);

            if (!Directory.Exists(targetDirPath))
            {
                Directory.CreateDirectory(targetDirPath);
            }

            foreach (string file in files)
            {
                string targetFileName = Path.GetFileName(file);
                File.Copy(file, Path.Join(targetDirPath, targetFileName), true);
            }

            Progress += percentPerFile;
        }
    }
}
