namespace PixiEditor.Models.ExceptionHandling;

public class CrashedFileInfo
{
    public string ZipName { get; set; }
    
    public string OriginalPath { get; set; }
    
    public CrashedFileInfo() { }

    public CrashedFileInfo(string zipName, string originalPath)
    {
        ZipName = zipName;
        OriginalPath = originalPath;
    }
}
