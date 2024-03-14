using System.IO;

namespace PixiEditor.Models.DataHolders;

public class CrashReportAutosaveFilePathInfo(string? originalPath, string? autosavePath)
{
    public string? OriginalPath { get; set; } = originalPath;

    public string? AutosavePath { get; set; } = autosavePath;
    
}
