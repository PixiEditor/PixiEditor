namespace PixiEditor.Models.DataHolders;

public class CrashFilePathInfo
{
    public string? OriginalPath { get; set; }
    
    public string? AutosavePath { get; set; }
    
    public CrashFilePathInfo(string originalPath, string autosavePath)
    {
        OriginalPath = originalPath;
        AutosavePath = autosavePath;
    }
}
