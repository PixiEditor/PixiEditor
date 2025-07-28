namespace PixiEditor.Models.ExceptionHandling;

public class CrashedFileInfo
{
    public string ZipName { get; set; }
    
    public string OriginalPath { get; set; }
    public string AutosavePath { get; set; }
    
    public CrashedFileInfo() { }

    public CrashedFileInfo(string zipName, string originalPath, string autosavePath)
    {
        ZipName = zipName;
        OriginalPath = originalPath;
        AutosavePath = autosavePath;
    }
}
