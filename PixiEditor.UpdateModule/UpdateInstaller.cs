using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace PixiEditor.UpdateModule
{
    public class UpdateInstaller
    {
        public const string TargetDirectoryName = "UpdateFiles";
        public static string UpdateFilesPath = Path.Join(UpdateDownloader.DownloadLocation, TargetDirectoryName);

        public event EventHandler<UpdateProgressChangedEventArgs> ProgressChanged;
        private float _progress = 0;
        public float Progress 
        {
            get => _progress;
            set
            {
                _progress = value;
                ProgressChanged?.Invoke(this, new UpdateProgressChangedEventArgs(value));
            }
        }
        public string ArchiveFileName { get; set; }
        public string TargetDirectory { get; set; }

        public UpdateInstaller(string archiveFileName, string targetDirectory)
        {
            ArchiveFileName = archiveFileName;
            TargetDirectory = targetDirectory;
        }

        public void Install()
        {
            var processes = Process.GetProcessesByName("PixiEditor");
            if (processes.Length > 0)
            {
                processes[0].WaitForExit();
            }
            ZipFile.ExtractToDirectory(ArchiveFileName, UpdateFilesPath, true);
            Progress = 25; //25% for unzip
            string dirWithFiles = Directory.GetDirectories(UpdateFilesPath)[0];
            string[] files = Directory.GetFiles(dirWithFiles);
            CopyFilesToDestination(files);
            DeleteArchive();
            Progress = 100;
        }

        private void DeleteArchive()
        {
            File.Delete(ArchiveFileName);
            Directory.Delete(UpdateFilesPath, true);
        }

        private void CopyFilesToDestination(string[] files)
        {
            float fileCopiedVal = 74f / files.Length; //74% is reserved for copying
            string destinationDir = TargetDirectory;
            foreach (string file in files)
            {
                string targetFileName = Path.GetFileName(file);
                File.Copy(file, Path.Join(destinationDir, targetFileName), true);
                Progress += fileCopiedVal;
            }
        }
    }
}
