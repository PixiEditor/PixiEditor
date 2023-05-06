using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using ByteSizeLib;
using Hardware.Info;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.Helpers;

internal class CrashHelper
{
    private readonly IHardwareInfo hwInfo;

    public static void SaveCrashInfo(Exception exception)
    {
        CrashReport report = CrashReport.Generate(exception);
        report.TrySave();
        report.RestartToCrashReport();
    }

    public CrashHelper()
    {
        hwInfo = new HardwareInfo();
    }

    public void GetCPUInformation(StringBuilder builder)
    {
        builder.AppendLine("CPU:");
        hwInfo.RefreshCPUList(false);

        foreach (var processor in hwInfo.CpuList)
        {
            builder
                .AppendLine($"  Name: {processor.Name}")
                .AppendLine($"  Speed: {(processor.CurrentClockSpeed / 1000f).ToString("F2", CultureInfo.InvariantCulture)} GHz")
                .AppendLine($"  Max Speed: {(processor.MaxClockSpeed / 1000f).ToString("F2", CultureInfo.InvariantCulture)} GHz")
                .AppendLine();
        }
    }

    public void GetGPUInformation(StringBuilder builder)
    {
        builder.AppendLine("GPU:");
        hwInfo.RefreshVideoControllerList();

        foreach (var gpu in hwInfo.VideoControllerList)
        {
            builder
                .AppendLine($"  Name: {gpu.Name}")
                .AppendLine($"  Driver: {gpu.DriverVersion}")
                .AppendLine();
        }
    }

    public void GetMemoryInformation(StringBuilder builder)
    {
        builder.AppendLine("Memory:");
        hwInfo.RefreshMemoryStatus();

        var memInfo = hwInfo.MemoryStatus;

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

    public static async Task SendExceptionInfoToWebhook(Exception e, [CallerFilePath] string filePath = "<unknown>", [CallerMemberName] string memberName = "<unknown>")
    {
        if (ViewModelMain.Current.DebugSubViewModel.IsDebugBuild)
            return;
        await SendReportTextToWebhook(CrashReport.Generate(e), $"{filePath}; Method {memberName}");
    }

    public static async Task SendReportTextToWebhook(CrashReport report, string catchLocation = null)
    {
        string reportText = report.ReportText;
        if (catchLocation is not null)
        {
            reportText = $"The report was generated from an exception caught in {catchLocation}.\r\n{reportText}";
        }

        byte[] bytes = Encoding.UTF8.GetBytes(reportText);
        string filename = Path.GetFileNameWithoutExtension(report.FilePath) + ".txt";

        MultipartFormDataContent formData = new MultipartFormDataContent
        {
            { new ByteArrayContent(bytes, 0, bytes.Length), "crash-report", filename }
        };
        try
        {
            using HttpClient httpClient = new HttpClient();
            string url = BuildConstants.CrashReportWebhookUrl;
            await httpClient.PostAsync(url, formData);
        }
        catch { }
    }
}
