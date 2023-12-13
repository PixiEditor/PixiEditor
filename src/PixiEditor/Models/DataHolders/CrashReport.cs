using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using PixiEditor.Helpers;
using PixiEditor.Parser;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.DataHolders;

#nullable enable
internal class CrashReport : IDisposable
{
    public static CrashReport Generate(Exception exception)
    {
        StringBuilder builder = new();
        DateTimeOffset currentTime = DateTimeOffset.Now;

        builder
            .AppendLine($"PixiEditor {VersionHelpers.GetCurrentAssemblyVersionString(moreSpecific: true)} x{IntPtr.Size * 8} crashed on {currentTime:yyyy.MM.dd} at {currentTime:HH:mm:ss} {currentTime:zzz}")
            .AppendLine($"Report: {Guid.NewGuid()}\n")
            .AppendLine("-----System Information----")
            .AppendLine("General:")
            .AppendLine($"  OS: {Environment.OSVersion.VersionString}")
            .AppendLine();

        CrashHelper helper = new();

        try
        {
            helper.GetCPUInformation(builder);
        }
        catch (Exception cpuE)
        {
            builder.AppendLine($"Error ({cpuE.GetType().FullName}: {cpuE.Message}) while gathering CPU information, skipping...");
        }

        try
        {
            helper.GetGPUInformation(builder);
        }
        catch (Exception gpuE)
        {
            builder.AppendLine($"Error ({gpuE.GetType().FullName}: {gpuE.Message}) while gathering GPU information, skipping...");
        }

        try
        {
            helper.GetMemoryInformation(builder);
        }
        catch (Exception memE)
        {
            builder.AppendLine($"Error ({memE.GetType().FullName}: {memE.Message}) while gathering memory information, skipping...");
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

    public int GetDocumentCount() => ZipFile.Entries.Where(x => x.FullName.EndsWith(".pixi")).Count();

    public List<(CrashFilePathInfo originalPath, byte[] dotPixiBytes)> RecoverDocuments()
    {
        // Load .pixi files
        Dictionary<string, byte[]> recoveredDocuments = new();
        foreach (ZipArchiveEntry entry in ZipFile.Entries.Where(x => x.FullName.EndsWith(".pixi")))
        {
            using Stream stream = entry.Open();
            using MemoryStream memStream = new();
            stream.CopyTo(memStream);
            recoveredDocuments.Add(entry.FullName["Documents/".Length..], memStream.ToArray());
        }

        var originalPathsEntry = ZipFile.Entries.First(entry => entry.FullName == "DocumentInfo.json");
        
        // Load original paths
        Dictionary<string, CrashFilePathInfo> originalPaths;
        {
            using Stream stream = originalPathsEntry.Open();
            using StreamReader reader = new(stream);
            string json = reader.ReadToEnd();
            originalPaths = JsonConvert.DeserializeObject<Dictionary<string, CrashFilePathInfo>>(json);
        }

        var list = new List<(CrashFilePathInfo originalPath, byte[] dotPixiBytes)>();

        foreach (var document in recoveredDocuments)
        {
            var originalPath = originalPaths[document.Key];
            list.Add((originalPath, document.Value));
        }

        return list;
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
        Dictionary<string, CrashFilePathInfo> originalPaths = new();
        foreach (DocumentViewModel document in vm.DocumentManagerSubViewModel.Documents)
        {
            try
            {
                string fileName = string.IsNullOrWhiteSpace(document.FullFilePath) ? "Unsaved" : Path.GetFileNameWithoutExtension(document.FullFilePath);
                string nameInZip = $"{fileName}-{document.OpenedUTC.ToString(CultureInfo.InvariantCulture)}-{counter.ToString(CultureInfo.InvariantCulture)}.pixi"
                    .Replace(':', '_')
                    .Replace('/', '_');

                byte[] serialized = PixiParser.Serialize(document.ToSerializable());

                using Stream documentStream = archive.CreateEntry($"Documents/{nameInZip}").Open();
                documentStream.Write(serialized);

                originalPaths.Add(nameInZip, new CrashFilePathInfo(document.FullFilePath, null));
            }
            catch { }
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

    internal class CrashReportUserMessage
    {
        public string Message { get; set; }

        public string Mail { get; set; }
    }
}
