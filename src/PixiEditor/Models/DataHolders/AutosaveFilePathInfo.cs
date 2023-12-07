using System.IO;

namespace PixiEditor.Models.DataHolders;

public class AutosaveFilePathInfo
{
    public string? OriginalPath { get; set; }
    
    public string? AutosavePath { get; set; }

    public Guid? GetAutosaveGuid()
    {
        if (AutosavePath == null)
            return null;
        
        string guidString = Path.GetFileNameWithoutExtension(AutosavePath)["autosave-".Length..];
        return Guid.Parse(guidString);
    }
    
    public AutosaveFilePathInfo(string? originalPath, string? autosavePath)
    {
        OriginalPath = originalPath;
        AutosavePath = autosavePath;
    }
}
