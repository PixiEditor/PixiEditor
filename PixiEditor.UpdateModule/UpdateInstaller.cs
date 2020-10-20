using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace PixiEditor.UpdateModule
{
    public class UpdateInstaller
    {
        public const string TargetDirectoryName = "UpdateFiles";
        private float progress;

        public UpdateInstaller(string archiveFileName)
        {
            ArchiveFileName = archiveFileName;
        }

        public event EventHandler<UpdateProgressChangedEventArgs> ProgressChanged;

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

        public void Install()
        {
            var processes = Process.GetProcessesByName("PixiEditor");
            if (processes.Length > 0)
            {
                processes[0].WaitForExit();
            }

            ZipFile.ExtractToDirectory(ArchiveFileName, TargetDirectoryName, true);
            Progress = 25; // 25% for unzip
            var dirWithFiles = Directory.GetDirectories(TargetDirectoryName)[0];
            var files = Directory.GetFiles(dirWithFiles);
            CopyFilesToDestination(files);
            DeleteArchive();
            Progress = 100;
        }

        private void DeleteArchive()
        {
            File.Delete(ArchiveFileName);
        }

        private void CopyFilesToDestination(string[] files)
        {
            var fileCopiedVal = 74f / files.Length; // 74% is reserved for copying
            var destinationDir = Path.GetDirectoryName(ArchiveFileName);
            foreach (var file in files)
            {
                var targetFileName = Path.GetFileName(file);
                File.Copy(file, Path.Join(destinationDir, targetFileName), true);
                Progress += fileCopiedVal;
            }
        }
    }
}