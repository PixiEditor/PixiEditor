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
            CrashReport report = CrashReport.Generate(exception);
            report.TrySave();
            report.RestartToCrashReport();
        }

        public static void GetCPUInformation(StringBuilder builder)
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

        public static void GetGPUInformation(StringBuilder builder)
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

        public static void GetMemoryInformation(StringBuilder builder)
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

        public static void AddExceptionMessage(StringBuilder builder, Exception e)
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