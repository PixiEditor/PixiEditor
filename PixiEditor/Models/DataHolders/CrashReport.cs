using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Parser;
using PixiEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PixiEditor.Models.DataHolders
{
    public class CrashReport : IDisposable
    {
        public static CrashReport Generate(Exception exception)
        {
            StringBuilder builder = new();
            DateTime currentTime = DateTime.Now;

            builder
                .AppendLine($"PixiEditor crashed on {currentTime:yyyy.MM.dd} at {currentTime:HH:mm:ss}\n")
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

        public IEnumerable<Document> RecoverDocuments()
        {
            foreach (ZipArchiveEntry entry in ZipFile.Entries.Where(x => x.FullName.EndsWith(".pixi")))
            {
                using Stream stream = entry.Open();

                Document document;

                try
                {
                    document = PixiParser.Deserialize(stream).ToDocument();
                    document.ChangesSaved = false;
                }
                catch
                {
                    continue;
                }

                yield return document;
            }
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

            foreach (Document document in ViewModelMain.Current.BitmapManager.Documents)
            {
                try
                {
                    string documentPath =
                        $"{(string.IsNullOrWhiteSpace(document.DocumentFilePath) ? "Unsaved" : Path.GetFileNameWithoutExtension(document.DocumentFilePath))}-{document.OpenedUTC}.pixi";

                    byte[] serialized = PixiParser.Serialize(document.ToSerializable());

                    using Stream documentStream = archive.CreateEntry($"Documents/{documentPath}").Open();
                    documentStream.Write(serialized);
                }
                catch
                { }
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

        public class CrashReportUserMessage
        {
            public string Message { get; set; }

            public string Mail { get; set; }
        }
    }
}
