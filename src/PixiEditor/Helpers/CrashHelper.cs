using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ByteSizeLib;
using Hardware.Info;
using PixiEditor.Models.AnalyticsAPI;
using PixiEditor.Models.ExceptionHandling;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Helpers;

internal partial class CrashHelper
{
    private readonly IHardwareInfo hwInfo;

    public static void SaveCrashInfo(Exception exception, IEnumerable<DocumentViewModel> documents)
    {
        try
        {
            //TODO: proper implementation of Mouse.OverrideCursor = Cursors.Wait;
        }
        catch (Exception e)
        {
            exception = new AggregateException(exception, e);
        }
        
        var report = CrashReport.Generate(exception, null);
        report.TrySave(documents);
        report.RestartToCrashReport();
    }

    public CrashHelper()
    {
        hwInfo = new HardwareInfo();
    }

    public void GetCPUInformation(StringBuilder builder, ApiCrashReport report)
    {
        builder.AppendLine("CPU:");
        hwInfo.RefreshCPUList(false);

        report.SystemInformation["CPUs"] = hwInfo.CpuList.Select(x => new
        {
            x.Name,
            SpeedGhz = (x.CurrentClockSpeed / 1000f).ToString("F2", CultureInfo.InvariantCulture),
            MaxSpeedGhz = (x.MaxClockSpeed / 1000f).ToString("F2", CultureInfo.InvariantCulture)
        });
        
        foreach (var processor in hwInfo.CpuList)
        {
            builder
                .AppendLine($"  Name: {processor.Name}")
                .AppendLine($"  Speed: {(processor.CurrentClockSpeed / 1000f).ToString("F2", CultureInfo.InvariantCulture)} GHz")
                .AppendLine($"  Max Speed: {(processor.MaxClockSpeed / 1000f).ToString("F2", CultureInfo.InvariantCulture)} GHz")
                .AppendLine();
        }
    }

    public void GetGPUInformation(StringBuilder builder, ApiCrashReport report)
    {
        builder.AppendLine("GPU:");
        hwInfo.RefreshVideoControllerList();

        report.SystemInformation["GPUs"] = hwInfo.VideoControllerList.Select(x => new
        {
            x.Name,
            x.DriverVersion
        });
        
        foreach (var gpu in hwInfo.VideoControllerList)
        {
            builder
                .AppendLine($"  Name: {gpu.Name}")
                .AppendLine($"  Driver: {gpu.DriverVersion}")
                .AppendLine();
        }
    }

    public void GetMemoryInformation(StringBuilder builder, ApiCrashReport report)
    {
        builder.AppendLine("Memory:");
        hwInfo.RefreshMemoryStatus();

        var memInfo = hwInfo.MemoryStatus;

        report.SystemInformation["Memory"] = new
        {
            memInfo.AvailablePhysical,
            memInfo.TotalPhysical
        };
        
        builder
            .AppendLine($"  Available: {new ByteSize(memInfo.AvailablePhysical).ToString("", CultureInfo.InvariantCulture)}")
            .AppendLine($"  Total: {new ByteSize(memInfo.TotalPhysical).ToString("", CultureInfo.InvariantCulture)}");
    }

    public static void AddExceptionMessage(StringBuilder builder, Exception e)
    {
        builder
            .AppendLine("\n-------Crash message-------")
            .Append(e.GetType().ToString())
            .Append(": ")
            .AppendLine(TrimFilePaths(e.Message));
        {
            var innerException = e.InnerException;
            while (innerException != null)
            {
                builder
                    .Append("\n-----Inner exception-----\n")
                    .Append(innerException.GetType().ToString())
                    .Append(": ")
                    .Append(TrimFilePaths(innerException.Message));
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

    public static string TrimFilePaths(string text) => FilePathRegex().Replace(text, "{{ FILE PATH }}");
    
    public static void SendExceptionInfo(Exception e, bool wait = false,
        [CallerFilePath] string filePath = "<unknown>", [CallerMemberName] string memberName = "<unknown>")
    {
        // TODO: quadruple check that this Task.Run is actually acceptable here
        // I think it might not be because there is stuff about the main window in the crash report, so Avalonia is touched from a different thread (is it bad for avalonia?)
        var task = Task.Run(() => SendExceptionInfoAsync(e, filePath, memberName));
        if (wait)
        {
            task.Wait();
        }
    }

    public static async Task SendExceptionInfoAsync(Exception e, [CallerFilePath] string filePath = "<unknown>", [CallerMemberName] string memberName = "<unknown>")
    {
        // TODO: Proper DebugBuild checking
        /*if (DebugViewModel.IsDebugBuild)
            return;*/

        var report = CrashReport.Generate(e, new NonCrashInfo(filePath, memberName));
        
        await SendReportToAnalyticsApiAsync(report);
    }

    public static async Task SendReportToAnalyticsApiAsync(CrashReport report)
    {
        if (AnalyticsClient.GetAnalyticsUrl() is not { } analyticsUrl)
        {
            return;
        }

        try
        {
            using var analyticsClient = new AnalyticsClient(analyticsUrl);
            await analyticsClient.SendReportAsync(report.ApiReportJson);
        }
        catch { }
    }

    /// <summary>
    /// Matches file paths with spaces when in quotes, otherwise not
    /// </summary>
    [GeneratedRegex(@"'([^']*[\/\\][^']*)'|(\S*[\/\\]\S*)")]
    private static partial Regex FilePathRegex();
}
