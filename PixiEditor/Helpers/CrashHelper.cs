using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Parser;
using PixiEditor.ViewModels;
using System;
using System.IO;
using System.IO.Compression;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace PixiEditor.Helpers
{
    public static class CrashHelper
    {
        public static void SaveCrashInfo(Exception exception)
        {
            StringBuilder builder = new();
            DateTime currentTime = DateTime.Now;

            builder
                .AppendLine($"PixiEditor crashed on {currentTime:yyyy.MM.dd} at {currentTime:HH:mm:ss}\n")
                .AppendLine("-----System Information----")
                .AppendLine("General:")
                .AppendLine($"  OS: {Environment.OSVersion.VersionString}")
                .AppendLine();

            try
            {
                GetCPUInformation(builder);
            }
            catch (Exception cpuE)
            {
                builder.AppendLine($"Error ({cpuE.GetType().FullName}: {cpuE.Message}) while gathering CPU information, skipping...");
            }

            try
            {
                GetGPUInformation(builder);
            }
            catch (Exception gpuE)
            {
                builder.AppendLine($"Error ({gpuE.GetType().FullName}: {gpuE.Message}) while gathering GPU information, skipping...");
            }

            try
            {
                GetMemoryInformation(builder);
            }
            catch (Exception memE)
            {
                builder.AppendLine($"Error ({memE.GetType().FullName}: {memE.Message}) while gathering memory information, skipping...");
            }

            AddExceptionMessage(builder, exception);

            string filename = $"crash-{currentTime:yyyy-MM-dd_HH-mm-ss_fff}.zip";
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PixiEditor",
                "crash_logs");
            Directory.CreateDirectory(path);

            string filePath = Path.Combine(path, filename);
            string report = builder.ToString();

            try
            {
                CreateZip(filePath, report);
            }
            catch
            {
                File.WriteAllText(Path.ChangeExtension(filePath, ".txt"), report);
            }
        }

        private static void GetCPUInformation(StringBuilder builder)
        {
            builder.AppendLine("CPU:");

            ManagementClass processorClass = new("Win32_Processor");
            ManagementObjectCollection processorsCollection = processorClass.GetInstances();

            foreach (var processor in processorsCollection)
            {
                builder
                    .AppendLine($"  ID: {processor.Properties["DeviceID"].Value}")
                    .AppendLine($"  Name: {processor.Properties["Name"].Value}");
            }
        }

        private static void GetGPUInformation(StringBuilder builder)
        {
            builder.AppendLine("\nGPU:");

            ManagementClass gpuClass = new("Win32_VideoController");
            ManagementObjectCollection gpuCollection = gpuClass.GetInstances();

            foreach (var gpu in gpuCollection)
            {
                builder
                    .AppendLine($"  ID: {gpu.Properties["DeviceID"].Value}")
                    .AppendLine($"  Name: {gpu.Properties["Name"].Value}");
            }
        }

        private static void GetMemoryInformation(StringBuilder builder)
        {
            builder.AppendLine("\nMemory:");

            // TODO: Make this work
            if (TryGetMemoryStatus(out MemoryStatus status))
            {
                builder.AppendLine($"  Usage: {status.dwMemoryLoad}%");
                builder.AppendLine($"  Available Memory: {status.ullAvailPhys}");
                builder.AppendLine($"  Total Memory: {status.ullTotalPhys}");
            }
            else
            {
                throw new InvalidOperationException($"Getting memory failed: {Marshal.GetLastWin32Error()}");
            }
        }

        private static void AddExceptionMessage(StringBuilder builder, Exception e)
        {

            builder
                .AppendLine("\n-------Crash message-------")
                .Append(e.GetType().ToString())
                .Append(": ")
                .AppendLine(e.Message);
            {
                var innerException = e.InnerException;
                while (innerException != null)
                {
                    builder
                        .Append("\n-----Inner exception-----\n")
                        .Append(innerException.GetType().ToString())
                        .Append(": ")
                        .Append(innerException.Message);
                    innerException = innerException.InnerException;
                }
            }

            builder
                .Append("\n\n-------Stack trace-------\n")
                .Append(e.StackTrace);
            {
                var innerException = e.InnerException;
                while (innerException != null)
                {
                    builder
                        .Append("\n-----Inner exception-----\n")
                        .Append(innerException.StackTrace);
                    innerException = innerException.InnerException;
                }
            }
        }

        private static void CreateZip(string filePath, string report)
        {
            using FileStream zipStream = new(/*Path.Combine(path, filename)*/filePath, FileMode.Create, FileAccess.Write);
            using ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create);

            using (Stream reportStream = archive.CreateEntry("report.txt").Open())
            {
                reportStream.Write(Encoding.UTF8.GetBytes(report));
            }

            foreach (Document document in ViewModelMain.Current.BitmapManager.Documents)
            {
                try
                {
                    string documentPath =
                        $"{(string.IsNullOrWhiteSpace(document.DocumentFilePath) ? "Unsaved" : Path.GetFileNameWithoutExtension(document.DocumentFilePath))}-{document.OpenedUTC}.pixi";
                    using Stream documentStream = archive.CreateEntry($"Documents/{documentPath}").Open();

                    PixiParser.Serialize(document.ToSerializable(), documentStream);
                }
                catch
                { }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MemoryStatus
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        private static unsafe bool TryGetMemoryStatus(out MemoryStatus status)
        {
            MemoryStatus memoryStatus = new();
            memoryStatus.dwLength = (uint)sizeof(MemoryStatus);

            bool success = GlobalMemoryStatusEx(memoryStatus);

            status = memoryStatus;

            return success;
        }

        // https://docs.microsoft.com/en-us/windows/win32/api/sysinfoapi/nf-sysinfoapi-globalmemorystatusex
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatus lpBuffer);
    }
}