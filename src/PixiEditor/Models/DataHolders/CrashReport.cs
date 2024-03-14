using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using Newtonsoft.Json;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Enums;
using PixiEditor.Parser;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.Views;

namespace PixiEditor.Models.DataHolders;

#nullable enable
internal class CrashReport : IDisposable
{
    public static CrashReport Generate(Exception exception)
    {
        StringBuilder builder = new();
        DateTimeOffset currentTime = DateTimeOffset.Now;

        builder
            .AppendLine(
                $"PixiEditor {VersionHelpers.GetCurrentAssemblyVersionString(moreSpecific: true)} x{IntPtr.Size * 8} crashed on {currentTime:yyyy.MM.dd} at {currentTime:HH:mm:ss} {currentTime:zzz}")
            .AppendLine(
                $"Application started {GetFormatted(() => Process.GetCurrentProcess().StartTime, "yyyy.MM.dd HH:hh:ss")}, {GetFormatted(() => DateTime.Now - Process.GetCurrentProcess().StartTime, @"d\ hh\:mm\.ss")} ago")
            .AppendLine($"Report: {Guid.NewGuid()}\n")
            .AppendLine("-----System Information----")
            .AppendLine("General:")
            .AppendLine($"  OS: {Environment.OSVersion.VersionString}")
            .AppendLine($"  Has Stylus Tablet Device: {GetFormatted(() => HasTabletDevice(TabletDeviceType.Stylus))}")
            .AppendLine($"  Has Touch Tablet Device: {GetFormatted(() => HasTabletDevice(TabletDeviceType.Touch))}")
            .AppendLine();

        CrashHelper helper = new();

        AppendHardwareInfo(helper, builder);

        builder.AppendLine("\n--------Command Log--------\n");

        try
        {
            builder.Append(CommandController.Current.Log.GetSummary(currentTime.LocalDateTime));
        }
        catch (Exception cemLogException)
        {
            builder.AppendLine(
                $"Error ({cemLogException.GetType().FullName}: {cemLogException.Message}) while gathering command log, skipping...");
        }

        builder.AppendLine("\n-----------State-----------");

        try
        {
            AppendStateInfo(builder);
        }
        catch (Exception stateException)
        {
            builder.AppendLine(
                $"Error ({stateException.GetType().FullName}: {stateException.Message}) while gathering state (Must be bug in GetPreferenceFormatted, GetFormatted or StringBuilder.AppendLine as these should not throw), skipping...");
        }

        CrashHelper.AddExceptionMessage(builder, exception);

        string filename = $"crash-{currentTime:yyyy-MM-dd_HH-mm-ss_fff}.zip";
        string path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PixiEditor",
            "crash_logs");
        Directory.CreateDirectory(path);

        CrashReport report = new();
        report.FilePath = Path.Combine(path, filename);
        report.ReportText = builder.ToString();

        return report;
    }

    private static void AppendHardwareInfo(CrashHelper helper, StringBuilder builder)
    {
        try
        {
            helper.GetCPUInformation(builder);
        }
        catch (Exception cpuE)
        {
            builder.AppendLine(
                $"Error ({cpuE.GetType().FullName}: {cpuE.Message}) while gathering CPU information, skipping...");
        }

        try
        {
            helper.GetGPUInformation(builder);
        }
        catch (Exception gpuE)
        {
            builder.AppendLine(
                $"Error ({gpuE.GetType().FullName}: {gpuE.Message}) while gathering GPU information, skipping...");
        }

        try
        {
            helper.GetMemoryInformation(builder);
        }
        catch (Exception memE)
        {
            builder.AppendLine(
                $"Error ({memE.GetType().FullName}: {memE.Message}) while gathering memory information, skipping...");
        }
    }

    private static void AppendStateInfo(StringBuilder builder)
    {
        builder
            .AppendLine("Environment:")
            .AppendLine($"  Thread Count: {GetFormatted(() => Process.GetCurrentProcess().Threads.Count)}")
            .AppendLine("\nCulture:")
            .AppendLine($"  Selected language: {GetPreferenceFormatted("LanguageCode", true, "system")}")
            .AppendLine($"  Current Culture: {GetFormatted(() => CultureInfo.CurrentCulture)}")
            .AppendLine($"  Current UI Culture: {GetFormatted(() => CultureInfo.CurrentUICulture)}")
            .AppendLine("\nPreferences:")
            .AppendLine($"  Has shared toolbar enabled: {GetPreferenceFormatted("EnableSharedToolbar", true, false)}")
            .AppendLine($"  Right click mode: {GetPreferenceFormatted<RightClickMode>("RightClickMode", true)}")
            .AppendLine($"  Has Rich presence enabled: {GetPreferenceFormatted("EnableRichPresence", true, true)}")
            .AppendLine($"  Autosaving Enabled: {GetPreferenceFormatted(PreferencesConstants.AutosaveEnabled, true, PreferencesConstants.AutosaveEnabledDefault)}")
            .AppendLine($"  Autosaving Period: {GetPreferenceFormatted(PreferencesConstants.AutosavePeriodMinutes, true, PreferencesConstants.AutosavePeriodDefault)}")
            .AppendLine($"  Debug Mode enabled: {GetPreferenceFormatted("IsDebugModeEnabled", true, false)}")
            .AppendLine("\nUI:")
            .AppendLine($"  MainWindow not null: {GetFormatted(() => MainWindow.Current != null)}")
            .AppendLine($"  MainWindow Size: {GetFormatted(() => MainWindow.Current.RenderSize)}")
            .AppendLine($"  MainWindow State: {GetFormatted(() => MainWindow.Current.WindowState)}")
            .AppendLine("\nViewModels:")
            .AppendLine(
                $"  Has active updateable change: {GetFormatted(() => ViewModelMain.Current?.DocumentManagerSubViewModel?.ActiveDocument?.UpdateableChangeActive)}")
            .AppendLine(
                $"  Current Tool: {GetFormattedFromViewModelMain(x => x.ToolsSubViewModel?.ActiveTool?.ToolName)}")
            .AppendLine($"  Primary Color: {GetFormattedFromViewModelMain(x => x.ColorsSubViewModel?.PrimaryColor)}")
            .AppendLine(
                $"  Secondary Color: {GetFormattedFromViewModelMain(x => x.ColorsSubViewModel?.SecondaryColor)}")
            .Append("\nActive Document: ");

        try
        {
            AppendActiveDocumentInfo(builder);
        }
        catch (Exception e)
        {
            builder.AppendLine($"Could not get active document info:\n{e}");
        }
    }

    private static void AppendActiveDocumentInfo(StringBuilder builder)
    {
        var main = ViewModelMain.Current;

        if (main == null)
        {
            builder.AppendLine("{ ViewModelMain.Current is null }");
            return;
        }

        var manager = main.DocumentManagerSubViewModel;

        if (manager == null)
        {
            builder.AppendLine("{ DocumentManagerSubViewModel is null }");
            return;
        }

        var document = manager.ActiveDocument;

        if (document == null)
        {
            builder.AppendLine("null");
            return;
        }

        builder
            .AppendLine()
            .AppendLine($"  Size: {document.SizeBindable}")
            .AppendLine($"  Layer Count: {FormatObject(document.StructureHelper.GetAllLayers().Count)}")
            .AppendLine($"  Has all changes saved: {document.AllChangesSaved}")
            .AppendLine($"  Horizontal Symmetry Enabled: {document.HorizontalSymmetryAxisEnabledBindable}")
            .AppendLine($"  Horizontal Symmetry Value: {FormatObject(document.HorizontalSymmetryAxisYBindable)}")
            .AppendLine($"  Vertical Symmetry Enabled: {document.VerticalSymmetryAxisEnabledBindable}")
            .AppendLine($"  Vertical Symmetry Value: {FormatObject(document.VerticalSymmetryAxisXBindable)}")
            .AppendLine($"  Updateable Change Active: {FormatObject(document.UpdateableChangeActive)}")
            .AppendLine($"  Transform: {FormatObject(document.TransformViewModel)}");
    }

    private static bool HasTabletDevice(TabletDeviceType type) =>
        Tablet.TabletDevices.Cast<TabletDevice>().Any(tabletDevice => tabletDevice.Type == type);

    private static string GetPreferenceFormatted<T>(string name, bool roaming, T defaultValue = default,
        string? format = null)
    {
        try
        {
            var preferences = IPreferences.Current;

            if (preferences == null)
                return "{ Preferences are null }";

            var value = roaming
                ? preferences.GetPreference(name, defaultValue)
                : preferences.GetLocalPreference(name, defaultValue);

            return FormatObject(value, format);
        }
        catch (Exception e)
        {
            return $$"""{ Failed getting preference: {{e.Message}} }""";
        }
    }

    private static string GetFormattedFromViewModelMain<T>(Func<ViewModelMain, T?> getter, string? format = null)
    {
        var main = ViewModelMain.Current;

        if (main == null)
            return "{ ViewModelMain.Current is null }";

        return GetFormatted(() => getter(main), format);
    }

    private static string GetFormatted<T>(Func<T?> getter, string? format = null)
    {
        try
        {
            var value = getter();

            return FormatObject(value, format);
        }
        catch (Exception e)
        {
            return $$"""{ Failed retrieving: {{e.Message}} }""";
        }
    }

    private static string FormatObject<T>(T? value, string? format = null)
    {
        return value switch
        {
            null => "null",
            IFormattable formattable => formattable.ToString(format, CultureInfo.InvariantCulture),
            LocalizedString localizedS => FormatLocalizedString(localizedS),
            string s => $"\"{s}\"",
            _ => value.ToString()
        };

        string FormatLocalizedString(LocalizedString localizedS)
        {
            return localizedS.Parameters != null
                ? $"{localizedS.Key} @({string.Join(", ", localizedS.Parameters.Select(x => FormatObject(x, format)))})"
                : localizedS.Key;
        }
    }

    public static CrashReport Parse(string path)
    {
        CrashReport report = new();
        report.FilePath = path;

        report.ZipFile = System.IO.Compression.ZipFile.Open(path, ZipArchiveMode.Read);
        report.ExtractReport();

        return report;
    }

    public string FilePath { get; set; }

    public string ReportText { get; set; }

    private ZipArchive ZipFile { get; set; }

    public int GetDocumentCount() =>
        ZipFile.Entries.Count(x => x.FullName.StartsWith("Documents") && x.FullName.EndsWith(".pixi"));

    public List<RecoveredPixi> RecoverDocuments()
    {
        var originalPathsEntry = ZipFile.Entries.First(entry => entry.FullName == "DocumentInfo.json");

        // Load original paths
        Dictionary<string, CrashReportAutosaveFilePathInfo> paths;
        {
            using Stream stream = originalPathsEntry.Open();
            using StreamReader reader = new(stream);
            string json = reader.ReadToEnd();
            paths = JsonConvert.DeserializeObject<Dictionary<string, CrashReportAutosaveFilePathInfo>>(json);
        }

        // Load .pixi files
        List<RecoveredPixi> recoveredDocuments = new();
        foreach (var path in paths)
        {
            ZipArchiveEntry autosaved = null;
            if (path.Value.AutosavePath != null)
            {
                autosaved = ZipFile.GetEntry($"Autosave/{Path.GetFileName(path.Value.AutosavePath)}");
            }

            recoveredDocuments.Add(new RecoveredPixi(path.Value, ZipFile.GetEntry($"Documents/{path.Key}"), autosaved));
        }

        return recoveredDocuments;
    }

    public void Dispose()
    {
        ZipFile.Dispose();
    }

    public void RestartToCrashReport()
    {
        Process process = new();

        process.StartInfo = new()
        {
            FileName = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, "exe"),
            Arguments = $"--crash \"{Path.GetFullPath(FilePath)}\""
        };

        process.Start();
    }

    public bool TrySave()
    {
        try
        {
            Save();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Save()
    {
        using FileStream zipStream = new(FilePath, FileMode.Create, FileAccess.Write);
        using ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create);

        using (Stream reportStream = archive.CreateEntry("report.txt").Open())
        {
            reportStream.Write(Encoding.UTF8.GetBytes(ReportText));
        }

        var vm = ViewModelMain.Current;
        if (vm is null)
            return;

        // Write the documents into zip
        int counter = 0;
        Dictionary<string, CrashReportAutosaveFilePathInfo> originalPaths = new();
        foreach (DocumentViewModel document in vm.DocumentManagerSubViewModel.Documents)
        {
            try
            {
                string fileName = string.IsNullOrWhiteSpace(document.FullFilePath)
                    ? "Unsaved"
                    : Path.GetFileNameWithoutExtension(document.FullFilePath);
                string nameInZip =
                    $"{fileName}-{document.OpenedUTC.ToString(CultureInfo.InvariantCulture)}-{counter.ToString(CultureInfo.InvariantCulture)}.pixi"
                        .Replace(':', '_')
                        .Replace('/', '_');

                byte[] serialized = PixiParser.Serialize(document.ToSerializable());

                using Stream documentStream = archive.CreateEntry($"Documents/{nameInZip}").Open();
                documentStream.Write(serialized);

                originalPaths.Add(nameInZip,
                    new CrashReportAutosaveFilePathInfo(document.FullFilePath, document.AutosaveViewModel.LastAutosavedPath));
            }
            catch
            {
                // ignored
            }

            try
            {
                if (document.AutosaveViewModel.LastAutosavedPath != null)
                {
                    using var file = File.OpenRead(document.AutosaveViewModel.LastAutosavedPath);
                    using var entry = archive
                        .CreateEntry($"Autosave/{Path.GetFileName(document.AutosaveViewModel.LastAutosavedPath)}").Open();

                    file.CopyTo(entry);
                }
            }
            catch
            {
                // ignored
            }

            counter++;
        }

        // Write their original paths into a separate file
        {
            using Stream jsonStream = archive.CreateEntry("DocumentInfo.json").Open();
            using StreamWriter writer = new StreamWriter(jsonStream);

            string originalPathsJson = JsonConvert.SerializeObject(originalPaths, Formatting.Indented);
            writer.Write(originalPathsJson);
        }
    }

    private void ExtractReport()
    {
        ZipArchiveEntry entry = ZipFile.GetEntry("report.txt");
        using Stream stream = entry.Open();

        byte[] encodedReport = new byte[entry.Length];
        stream.Read(encodedReport);

        ReportText = Encoding.UTF8.GetString(encodedReport);
    }

    public class RecoveredPixi
    {
        public CrashReportAutosaveFilePathInfo Path { get; }

        public ZipArchiveEntry RecoveredEntry { get; }

        public ZipArchiveEntry? AutosaveEntry { get; }

        public byte[] GetRecoveredBytes()
        {
            var buffer = new byte[RecoveredEntry.Length];
            using var stream = RecoveredEntry.Open();

            stream.ReadExactly(buffer);

            return buffer;
        }

        public byte[] GetAutosaveBytes()
        {
            var buffer = new byte[AutosaveEntry.Length];
            using var stream = AutosaveEntry.Open();

            stream.ReadExactly(buffer);

            return buffer;
        }

        public RecoveredPixi(CrashReportAutosaveFilePathInfo path, ZipArchiveEntry recoveredEntry, ZipArchiveEntry? autosaveEntry)
        {
            Path = path;
            RecoveredEntry = recoveredEntry;
            AutosaveEntry = autosaveEntry;
        }
    }

    internal class CrashReportUserMessage
    {
        public string Message { get; set; }

        public class RecoveredPixi
        {
            public string? Path { get; }

            public ZipArchiveEntry RecoveredEntry { get; }

            public byte[] GetRecoveredBytes()
            {
                var buffer = new byte[RecoveredEntry.Length];
                using var stream = RecoveredEntry.Open();

                stream.ReadExactly(buffer);

                return buffer;
            }

            public RecoveredPixi(string? path, ZipArchiveEntry recoveredEntry)
            {
                Path = path;
                RecoveredEntry = recoveredEntry;
            }
        }
    }
}
