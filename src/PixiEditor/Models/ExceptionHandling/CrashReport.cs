using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.Json;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Helpers;
#if LINUX
using PixiEditor.Linux;
#endif
using PixiEditor.Models.AnalyticsAPI;
using PixiEditor.Models.Commands;
using PixiEditor.Models.DocumentModels.Autosave;
using PixiEditor.OperatingSystem;
using PixiEditor.Parser;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.Document;
using PixiEditor.Views;

namespace PixiEditor.Models.ExceptionHandling;

#nullable enable
internal class CrashReport : IDisposable
{
    public static CrashReport Generate(Exception exception, NonCrashInfo? nonCrashInfo)
    {
        var apiReport = new ApiCrashReport();
        StringBuilder builder = new();
        DateTimeOffset currentTime = DateTimeOffset.Now;
        var processStartTime = Process.GetCurrentProcess().StartTime;

        apiReport.Version = VersionHelpers.GetCurrentAssemblyVersion();
        apiReport.BuildId = VersionHelpers.GetBuildId();

        apiReport.ReportTime = currentTime.UtcDateTime;
        apiReport.ProcessStart = processStartTime.ToUniversalTime();
        apiReport.IsCrash = nonCrashInfo == null;

        if (nonCrashInfo != null)
        {
            apiReport.CatchLocation = nonCrashInfo.CatchLocation;
            apiReport.CatchMethod = nonCrashInfo.CatchMember;
        }

        try
        {
            var os = IOperatingSystem.Current;

            apiReport.SystemInformation["PlatformId"] = os.AnalyticsId;
            apiReport.SystemInformation["PlatformName"] = os.AnalyticsName;
            apiReport.SystemInformation["OSVersion"] = Environment.OSVersion.VersionString;
#if LINUX
            if (os is LinuxOperatingSystem linux)
            {
                apiReport.SystemInformation["DesktopEnvironment"] = linux.GetActiveDesktopEnvironment();
            }
#endif
        }
        catch (Exception e)
        {
            exception = new AggregateException(exception, new CrashInfoCollectionException("OS Information", e));
        }

        try
        {
            var sessionId = AnalyticsPeriodicReporter.Instance?.SessionId;

            if (sessionId == Guid.Empty) sessionId = null;

            apiReport.SessionId = sessionId;
        }
        catch (Exception e)
        {
            exception = new AggregateException(exception, new CrashInfoCollectionException("Session Id", e));
        }

        builder
            .AppendLine(
                $"PixiEditor {VersionHelpers.GetCurrentAssemblyVersionString(moreSpecific: true)} x{IntPtr.Size * 8} crashed on {currentTime:yyyy.MM.dd} at {currentTime:HH:mm:ss} {currentTime:zzz}")
            .AppendLine(
                $"Application started {GetFormatted(() => processStartTime, "yyyy.MM.dd HH:hh:ss")}, {GetFormatted(() => currentTime - processStartTime, @"d\ hh\:mm\.ss")} ago")
            .AppendLine($"Report: {Guid.NewGuid()}\n")
            .AppendLine("-----System Information----")
            .AppendLine("General:");
        foreach (var sysInfo in apiReport.SystemInformation)
        {
            builder.AppendLine($"  {sysInfo.Key}: {sysInfo.Value}");
        }

        builder.AppendLine();

        CrashHelper helper = new();

        AppendHardwareInfo(helper, builder, apiReport);

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
            AppendStateInfo(builder, apiReport);
        }
        catch (Exception stateException)
        {
            exception = new AggregateException(exception,
                new CrashInfoCollectionException("state information", stateException));
            builder.AppendLine(
                $"Error ({stateException.GetType().FullName}: {stateException.Message}) while gathering state (Must be bug in GetPreferenceFormatted, GetFormatted or StringBuilder.AppendLine as these should not throw), skipping...");
        }

        apiReport.Exception = new ExceptionDetails(exception);
        CrashHelper.AddExceptionMessage(builder, exception);

        string filename = $"crash-{currentTime:yyyy-MM-dd_HH-mm-ss_fff}.zip";
        string path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PixiEditor",
            "crash_logs");
        Directory.CreateDirectory(path);

        CrashReport report = new();
        report.FilePath = Path.Combine(path, filename);
        try
        {
            report.ApiReportJson = System.Text.Json.JsonSerializer.Serialize(apiReport);
        }
        catch (Exception apiReportSerializationException)
        {
            // TODO: Handle this using the API once webhook reports are no longer a thing
            builder.AppendLine($"-- API Report Json Exception --");
            CrashHelper.AddExceptionMessage(builder, apiReportSerializationException);
        }

        report.ReportText = builder.ToString();

        return report;
    }

    private static void AppendHardwareInfo(CrashHelper helper, StringBuilder builder, ApiCrashReport apiReport)
    {
        try
        {
            helper.GetCPUInformation(builder, apiReport);
        }
        catch (Exception cpuE)
        {
            builder.AppendLine(
                $"Error ({cpuE.GetType().FullName}: {cpuE.Message}) while gathering CPU information, skipping...");
        }

        try
        {
            helper.GetGPUInformation(builder, apiReport);
        }
        catch (Exception gpuE)
        {
            builder.AppendLine(
                $"Error ({gpuE.GetType().FullName}: {gpuE.Message}) while gathering GPU information, skipping...");
        }


        try
        {
            helper.GetMemoryInformation(builder, apiReport);
        }
        catch (Exception memE)
        {
            builder.AppendLine(
                $"Error ({memE.GetType().FullName}: {memE.Message}) while gathering memory information, skipping...");
        }
    }

    private static void AppendStateInfo(StringBuilder builder, ApiCrashReport apiReport)
    {
        builder
            .AppendLine("Environment:")
            .AppendLine($"  Thread Count: {GetFormatted(() => Process.GetCurrentProcess().Threads.Count)}")
            .AppendLine("Analytics:")
            .AppendLine($"  Analytics Id: {GetFormatted(() => AnalyticsPeriodicReporter.Instance?.SessionId)}")
            .AppendLine("\nCulture:")
            .AppendLine($"  Selected language: {GetPreferenceFormatted("LanguageCode", true, "system")}")
            .AppendLine($"  Current Culture: {GetFormatted(() => CultureInfo.CurrentCulture)}")
            .AppendLine($"  Current UI Culture: {GetFormatted(() => CultureInfo.CurrentUICulture)}")
            .AppendLine("\nPreferences:")
            .AppendLine($"  Has shared toolbar enabled: {GetPreferenceFormatted("EnableSharedToolbar", true, false)}")
            .AppendLine($"  Right click mode: {GetPreferenceFormatted<RightClickMode>("RightClickMode", true)}")
            .AppendLine($"  Has Rich presence enabled: {GetPreferenceFormatted("EnableRichPresence", true, true)}")
            .AppendLine(
                $"   Autosaving Enabled: {GetPreferenceFormatted(PreferencesConstants.AutosaveEnabled, true, PreferencesConstants.AutosaveEnabledDefault)}")
            .AppendLine($"  Debug Mode enabled: {GetPreferenceFormatted("IsDebugModeEnabled", true, false)}")
            .AppendLine("\nUI:")
            .AppendLine($"  MainWindow not null: {GetFormatted(() => MainWindow.Current != null)}")
            .AppendLine($"  MainWindow Size: {GetFormatted(() => MainWindow.Current?.Bounds)}")
            .AppendLine($"  MainWindow State: {GetFormatted(() => MainWindow.Current?.WindowState)}")
            .AppendLine("\nViewModels:")
            .AppendLine(
                $"  Has active updateable change: {GetFormatted(() => ViewModelMain.Current?.DocumentManagerSubViewModel?.ActiveDocument?.BlockingUpdateableChangeActive)}")
            .AppendLine(
                $"  Current Tool: {GetFormattedFromViewModelMain(x => x.ToolsSubViewModel?.ActiveTool?.ToolName)}")
            .AppendLine($"  Primary Color: {GetFormattedFromViewModelMain(x => x.ColorsSubViewModel?.PrimaryColor)}")
            .AppendLine(
                $"  Secondary Color: {GetFormattedFromViewModelMain(x => x.ColorsSubViewModel?.SecondaryColor)}")
            .Append("\nActive Document: ");

        apiReport.StateInformation["Environment"] = new
        {
            ThreadCount = GetOrExceptionMessage(() => Process.GetCurrentProcess().Threads.Count)
        };

        apiReport.StateInformation["Culture"] = new
        {
            SelectedLanguage = GetPreferenceFormatted("LanguageCode", true, "system"),
            CurrentCulture = GetFormatted(() => CultureInfo.CurrentCulture),
            CurrentUICulture = GetFormatted(() => CultureInfo.CurrentUICulture)
        };

        apiReport.StateInformation["Preferences"] = new
        {
            HasSharedToolbarEnabled = GetPreferenceFormatted("EnableSharedToolbar", true, false),
            RightClickMode = GetPreferenceFormatted<RightClickMode>("RightClickMode", true),
            HasRichPresenceEnabled = GetPreferenceOrExceptionMessage("EnableRichPresence", true, true),
            DebugModeEnabled = GetPreferenceOrExceptionMessage("IsDebugModeEnabled", true, false)
        };

        apiReport.StateInformation["UI"] = new
        {
            MainWindowNotNull = GetOrExceptionMessage(() => MainWindow.Current != null),
            MainWindowSize = GetOrExceptionMessage(() => GetSimplifiedRect(MainWindow.Current?.Bounds)),
            MainWindowState = GetFormatted(() => MainWindow.Current?.WindowState)
        };

        apiReport.StateInformation["ViewModels"] = new
        {
            HasActiveUpdateableChange =
                GetOrExceptionMessage(() =>
                    ViewModelMain.Current?.DocumentManagerSubViewModel?.ActiveDocument
                        ?.BlockingUpdateableChangeActive),
            CurrentTool =
                GetOrExceptionMessage(() => ViewModelMain.Current?.ToolsSubViewModel?.ActiveTool?.ToolName),
            PrimaryColor =
                GetOrExceptionMessage(() => ViewModelMain.Current?.ColorsSubViewModel?.PrimaryColor.ToString()),
            SecondaryColor = GetOrExceptionMessage(() =>
                ViewModelMain.Current?.ColorsSubViewModel?.SecondaryColor.ToString())
        };

        apiReport.StateInformation["ActiveDocument"] = new { };

        try
        {
            AppendActiveDocumentInfo(builder);
        }
        catch (Exception e)
        {
            builder.AppendLine($"Could not get active document info:\n{e}");
        }

        object GetSimplifiedRect(Avalonia.Rect? rect)
        {
            if (rect == null) return null;
            var nonNull = rect.Value;

            return new { Left = nonNull.Left, Top = nonNull.Top, Width = nonNull.Width, Height = nonNull.Height };
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
            .AppendLine($"  Has all changes autosaved: {document.AllChangesAutosaved}")
            .AppendLine($"  Horizontal Symmetry Enabled: {document.HorizontalSymmetryAxisEnabledBindable}")
            .AppendLine($"  Horizontal Symmetry Value: {FormatObject(document.HorizontalSymmetryAxisYBindable)}")
            .AppendLine($"  Vertical Symmetry Enabled: {document.VerticalSymmetryAxisEnabledBindable}")
            .AppendLine($"  Vertical Symmetry Value: {FormatObject(document.VerticalSymmetryAxisXBindable)}")
            .AppendLine($"  Updateable Change Active: {FormatObject(document.BlockingUpdateableChangeActive)}")
            .AppendLine($"  Transform: {FormatObject(document.TransformViewModel)}");
    }

    private static object GetPreferenceOrExceptionMessage<T>(string name, bool roaming, T defaultValue)
    {
        try
        {
            var preferences = IPreferences.Current;

            if (preferences == null)
                return "{ Preferences are null }";

            var value = roaming
                ? preferences.GetPreference(name, defaultValue)
                : preferences.GetLocalPreference(name, defaultValue);

            return value;
        }
        catch (Exception e)
        {
            return $$"""{ Failed getting preference: {{e.Message}} }""";
        }
    }

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

    private static object GetOrExceptionMessage<T>(Func<T?> getter)
    {
        try
        {
            return getter();
        }
        catch (Exception e)
        {
            return e.GetType().FullName;
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
            _ => value.ToString()!
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
        report.ExtractJsonReport();

        return report;
    }

    public string FilePath { get; set; }

    public string ReportText { get; set; }

    public string ApiReportJson { get; set; }

    private ZipArchive ZipFile { get; set; }

    public int GetDocumentCount() => ZipFile.Entries.Where(x => x.FullName.EndsWith(".pixi")).Count();

    public bool TryRecoverDocuments(out List<RecoveredPixi> list, out CrashedSessionInfo? sessionInfo)
    {
        try
        {
            list = RecoverDocuments(out sessionInfo);
        }
        catch (Exception e)
        {
            list = null;
            sessionInfo = null;
            CrashHelper.SendExceptionInfo(e);
            return false;
        }

        return true;
    }

    public List<RecoveredPixi> RecoverDocuments(out CrashedSessionInfo? sessionInfo)
    {
        List<RecoveredPixi> recoveredDocuments = new();

        sessionInfo = TryGetSessionInfo();
        if (sessionInfo == null)
        {
            return recoveredDocuments;
        }

        if (sessionInfo?.OpenedDocuments == null)
        {
            recoveredDocuments.AddRange(
                ZipFile.Entries
                    .Where(x =>
                        x.FullName.StartsWith("Documents") &&
                        x.FullName.EndsWith(".pixi"))
                    .Select(entry => new RecoveredPixi(null, null, entry)));

            return recoveredDocuments;
        }

        foreach (var doc in sessionInfo.OpenedDocuments)
        {
            recoveredDocuments.Add(new RecoveredPixi(doc.OriginalPath, doc.AutosavePath,
                ZipFile.GetEntry($"Documents/{doc.ZipName}")));
        }

        return recoveredDocuments;

        CrashedSessionInfo? TryGetSessionInfo()
        {
            var originalPathsEntry = ZipFile.Entries.FirstOrDefault(entry => entry.FullName == "DocumentInfo.json");

            if (originalPathsEntry == null)
                return null;

            try
            {
                using var stream = originalPathsEntry.Open();
                using var reader = new StreamReader(stream);
                string json = reader.ReadToEnd();

                return JsonSerializer.Deserialize<CrashedSessionInfo>(json);
            }
            catch
            {
                return null;
            }
        }
    }

    public void Dispose()
    {
        ZipFile.Dispose();
    }

    public void RestartToCrashReport()
    {
        string fileName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location) +
                          IOperatingSystem.Current.ExecutableExtension;
#if DEBUG
        if (!File.Exists(fileName))
            fileName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location) +
                       $".Desktop{IOperatingSystem.Current.ExecutableExtension}";
#endif

        IOperatingSystem.Current.ProcessUtility.ShellExecute(fileName, $"--crash \"{Path.GetFullPath(FilePath)}\"");
    }

    public bool TrySave(IEnumerable<DocumentViewModel> documents)
    {
        try
        {
            Save(documents);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Save(IEnumerable<DocumentViewModel> documents)
    {
        using FileStream zipStream = new(FilePath, FileMode.Create, FileAccess.Write);
        using ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create);

        using (Stream reportStream = archive.CreateEntry("report.txt").Open())
        {
            reportStream.Write(Encoding.UTF8.GetBytes(ReportText));
        }

        using (var reportStream = archive.CreateEntry("report.json").Open())
        {
            reportStream.Write(Encoding.UTF8.GetBytes(ApiReportJson));
        }

        // Write the documents into zip
        int counter = 0;
        var originalPaths = new List<CrashedFileInfo>();
        foreach (var document in documents)
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

                byte[] serialized = PixiParser.V5.Serialize(document.ToSerializable());

                using Stream documentStream = archive.CreateEntry($"Documents/{nameInZip}").Open();
                documentStream.Write(serialized);
                document.AutosaveViewModel.Autosave(AutosaveHistoryType.Crash);

                originalPaths.Add(new CrashedFileInfo(nameInZip, document.FullFilePath,
                    document.AutosaveViewModel.LastAutosavedPath));

            }
            catch { }

            counter++;
        }

        // Write their original paths into a separate file
        {
            using Stream jsonStream = archive.CreateEntry("DocumentInfo.json").Open();
            using StreamWriter writer = new StreamWriter(jsonStream);

            string originalPathsJson = JsonSerializer.Serialize(
                new CrashedSessionInfo(AnalyticsPeriodicReporter.Instance?.SessionId ?? Guid.Empty, originalPaths),
                JsonOptions.CasesInsensitiveIndented);
            writer.Write(originalPathsJson);
        }
    }

    private void ExtractReport()
    {
        ZipArchiveEntry entry = ZipFile.GetEntry("report.txt");
        using Stream stream = entry.Open();

        byte[] encodedReport = new byte[entry.Length];
        stream.ReadExactly(encodedReport);

        ReportText = Encoding.UTF8.GetString(encodedReport);
    }

    private void ExtractJsonReport()
    {
        ZipArchiveEntry entry = ZipFile.GetEntry("report.json");
        using Stream stream = entry.Open();

        byte[] encodedReport = new byte[entry.Length];
        stream.ReadExactly(encodedReport);

        ApiReportJson = Encoding.UTF8.GetString(encodedReport);
    }

    public class RecoveredPixi
    {
        public string? OriginalPath { get; }
        public string? AutosavePath { get; }

        public ZipArchiveEntry RecoveredEntry { get; }

        public byte[] GetRecoveredBytes()
        {
            var buffer = new byte[RecoveredEntry.Length];
            using var stream = RecoveredEntry.Open();

            stream.ReadExactly(buffer);

            return buffer;
        }

        public RecoveredPixi(string? originalPath, string? autosavePath, ZipArchiveEntry recoveredEntry)
        {
            OriginalPath = originalPath;
            AutosavePath = autosavePath;
            RecoveredEntry = recoveredEntry;
        }

        public byte[] TryGetAutoSaveBytes()
        {
            if (AutosavePath == null)
                return [];

            string autosavePixiFile = AutosavePath;
            if (!File.Exists(autosavePixiFile))
                return [];

            return File.ReadAllBytes(autosavePixiFile);
        }
    }
}
