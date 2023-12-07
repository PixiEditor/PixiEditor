namespace PixiEditor.Models.DataHolders;

public class AutosaveFilePathInfo
{
    public string? OriginalPath { get; set; }
    
    public string? AutosavePath { get; set; }
    
    public AutosaveFilePathInfo(string? originalPath, string? autosavePath)
    {
        OriginalPath = originalPath;
        AutosavePath = autosavePath;
    }
}
