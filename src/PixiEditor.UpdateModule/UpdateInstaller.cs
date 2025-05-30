using System;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace PixiEditor.UpdateModule;

public class UpdateInstaller
{
    public const string TargetDirectoryName = "UpdateFiles";

    public UpdateInstaller(string archiveFileName, string targetDirectory)
    {
        ArchiveFileName = archiveFileName;
        TargetDirectory = targetDirectory;
    }

    public static string UpdateFilesPath { get; set; } =
        Path.Join(UpdateDownloader.DownloadLocation, TargetDirectoryName);

    public string ArchiveFileName { get; set; }

    public string TargetDirectory { get; set; }

    public void Install(StringBuilder log)
    {
        var processes = Process.GetProcessesByName("PixiEditor.Desktop");
        processes = processes.Concat(Process.GetProcessesByName("PixiEditor")).ToArray();
        log.AppendLine($"Found {processes.Length} PixiEditor processes running.");
        if (processes.Length > 0)
        {
            log.AppendLine("Killing PixiEditor processes...");
            foreach (var process in processes)
            {
                try
                {
                    log.AppendLine($"Killing process {process.ProcessName} with ID {process.Id}");
                    process.Kill();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    log.AppendLine($"Failed to kill process {process.ProcessName} with ID {process.Id}: {ex.Message}");
                }
            }

            log.AppendLine("Processes killed.");
        }

        log.AppendLine("Extracting files");

        if (Directory.Exists(UpdateFilesPath))
        {
            Directory.Delete(UpdateFilesPath, true);
        }

        Directory.CreateDirectory(UpdateFilesPath);

        log.AppendLine($"Extracting {ArchiveFileName} to {UpdateFilesPath}");
        ZipFile.ExtractToDirectory(ArchiveFileName, UpdateFilesPath, true);

        string[] extractedFiles = Directory.GetFiles(UpdateFilesPath, "*", SearchOption.AllDirectories);
        log.AppendLine($"Extracted {extractedFiles.Length} files to {UpdateFilesPath}");
        log.AppendLine("Files extracted");

        string dirWithFiles = UpdateFilesPath;
        string binName = "PixiEditor.exe";
        if (!File.Exists(Path.Combine(UpdateFilesPath, binName)))
        {
            dirWithFiles = Directory.GetDirectories(UpdateFilesPath)[0];
        }

        log.AppendLine($"Copying files from {dirWithFiles} to {TargetDirectory}");

        try
        {
            CopyFilesToDestination(dirWithFiles, log);
        }
        catch (IOException ex)
        {
            log.AppendLine($"Error copying files: {ex.Message}. Retrying in 1 second...");
            System.Threading.Thread.Sleep(1000);
            CopyFilesToDestination(dirWithFiles, log);
        }

        log.AppendLine("Files copied");
        log.AppendLine("Deleting archive and update files");

        Cleanup(log);
    }

    private void Cleanup(StringBuilder logger)
    {
        File.Delete(ArchiveFileName);
        Directory.Delete(UpdateFilesPath, true);
        string updateLocationFile = Path.Join(Path.GetTempPath(), "PixiEditor", "update-location.txt");
        logger.AppendLine($"Looking for: {updateLocationFile}");
        if (File.Exists(updateLocationFile))
        {
            try
            {
                logger.AppendLine($"Deleting update location file: {updateLocationFile}");
                File.Delete(updateLocationFile);
            }
            catch (Exception ex)
            {
                logger.AppendLine($"Failed to delete update location file: {ex.Message}");
            }
        }

        string updateInstallerFile = Path.Join(Path.GetTempPath(), "PixiEditor",
            "PixiEditor.UpdateInstaller.exe");
        logger.AppendLine($"Looking for: {updateInstallerFile}");
        if (File.Exists(updateInstallerFile))
        {
            try
            {
                logger.AppendLine($"Deleting update installer file: {updateInstallerFile}");
                File.Delete(updateInstallerFile);
            }
            catch (Exception ex)
            {
                logger.AppendLine($"Failed to delete update installer file: {ex.Message}");
            }
        }
    }

    private void CopyFilesToDestination(string sourceDirectory, StringBuilder log)
    {
        int totalFiles = Directory.GetFiles(UpdateFilesPath, "*", SearchOption.AllDirectories).Length;
        log.AppendLine($"Found {totalFiles} files to copy.");

        string[] files = Directory.GetFiles(sourceDirectory);

        foreach (string file in files)
        {
            string targetFileName = Path.GetFileName(file);
            string targetFilePath = Path.Join(TargetDirectory, targetFileName);
            log.AppendLine($"Copying {file} to {targetFilePath}");
            File.Copy(file, targetFilePath, true);
        }

        CopySubDirectories(sourceDirectory, TargetDirectory, log);
    }

    private void CopySubDirectories(string originDirectory, string targetDirectory, StringBuilder log)
    {
        string[] subDirs = Directory.GetDirectories(originDirectory);
        log.AppendLine($"Found {subDirs.Length} subdirectories to copy.");
        if (subDirs.Length == 0) return;

        foreach (string subDir in subDirs)
        {
            string targetDirPath = Path.Join(targetDirectory, Path.GetFileName(subDir));

            log.AppendLine($"Copying {subDir} to {targetDirPath}");

            CopySubDirectories(subDir, targetDirPath, log);

            string[] files = Directory.GetFiles(subDir);

            if (!Directory.Exists(targetDirPath))
            {
                Directory.CreateDirectory(targetDirPath);
            }

            foreach (string file in files)
            {
                string targetFileName = Path.GetFileName(file);
                log.AppendLine($"Copying {file} to {Path.Join(targetDirPath, targetFileName)}");
                File.Copy(file, Path.Join(targetDirPath, targetFileName), true);
            }
        }
    }
}
