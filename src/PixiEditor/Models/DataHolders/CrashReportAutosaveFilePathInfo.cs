using System.IO;
using PixiEditor.Helpers;

namespace PixiEditor.Models.DataHolders;

public class CrashReportAutosaveFilePathInfo(string? originalPath, string? autosavePath)
{
    public string? OriginalPath { get; set; } = originalPath;

    public string? AutosavePath { get; set; } = autosavePath;

    public Guid? GetAutosaveGuid()
    {
        return AutosaveHelper.GetAutosaveGuid(AutosavePath);
    }
}
