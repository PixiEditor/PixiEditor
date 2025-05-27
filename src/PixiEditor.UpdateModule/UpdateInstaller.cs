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

        bool isZip = ArchiveFileName.EndsWith(".zip");
        if (isZip)
        {
            log.AppendLine($"Extracting {ArchiveFileName} to {UpdateFilesPath}");
            ZipFile.ExtractToDirectory(ArchiveFileName, UpdateFilesPath, true);
        }
        else
        {
            log.AppendLine($"Extracting {ArchiveFileName} to {UpdateFilesPath} using GZipStream");
            using FileStream fs = new(ArchiveFileName, FileMode.Open, FileAccess.Read);
            using GZipStream gz = new(fs, CompressionMode.Decompress, leaveOpen: true);

            TarFile.ExtractToDirectory(gz, UpdateFilesPath, overwriteFiles: true);
        }

        if (OperatingSystem.IsMacOS())
        {
            string appFile = Directory.GetDirectories(UpdateFilesPath, "PixiEditor.app", SearchOption.TopDirectoryOnly)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(appFile))
            {
                log.AppendLine("PixiEditor.app not found in the update files. Installation failed.");
                string[] allFiles = Directory.GetFiles(UpdateFilesPath, "*.*", SearchOption.TopDirectoryOnly);
                foreach (string file in allFiles)
                {
                    log.AppendLine($"Found file: {file}");
                }
                throw new FileNotFoundException("PixiEditor.app not found in the update files.");
            }

            
            log.AppendLine($"Moving {appFile} to {TargetDirectory}");
            string targetAppDirectory = Path.Combine(TargetDirectory, "PixiEditor.app");
            if (Directory.Exists(targetAppDirectory))
            {
                log.AppendLine($"Removing existing PixiEditor.app at {targetAppDirectory}");
                Directory.Delete(targetAppDirectory, true);
            }
            Directory.Move(appFile, targetAppDirectory);
            
            DeleteArchive();
            return;
        }

        string[] extractedFiles = Directory.GetFiles(UpdateFilesPath, "*", SearchOption.AllDirectories);
        log.AppendLine($"Extracted {extractedFiles.Length} files to {UpdateFilesPath}");
        log.AppendLine("Files extracted");

        string dirWithFiles = UpdateFilesPath;
        string binName = OperatingSystem.IsWindows() ? "PixiEditor.exe" : "PixiEditor";
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
        
        DeleteArchive();
    }

    private void DeleteArchive()
    {
        File.Delete(ArchiveFileName);
        Directory.Delete(UpdateFilesPath, true);
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
