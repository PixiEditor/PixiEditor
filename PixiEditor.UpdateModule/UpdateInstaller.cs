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
            Process[] processes = Process.GetProcessesByName("PixiEditor");
            if (processes.Length > 0)
            {
                processes[0].WaitForExit();
            }

            ZipFile.ExtractToDirectory(ArchiveFileName, TargetDirectoryName, true);
            Progress = 25; // 25% for unzip
            string dirWithFiles = Directory.GetDirectories(TargetDirectoryName)[0];
            string[] files = Directory.GetFiles(dirWithFiles);
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
            float fileCopiedVal = 74f / files.Length; // 74% is reserved for copying
            string destinationDir = Path.GetDirectoryName(ArchiveFileName);
            foreach (string file in files)
            {
                string targetFileName = Path.GetFileName(file);
                File.Copy(file, Path.Join(destinationDir, targetFileName), true);
                Progress += fileCopiedVal;
            }
        }
    }
}