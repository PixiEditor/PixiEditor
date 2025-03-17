namespace PixiEditor.Models.DocumentModels.Autosave;

public struct SessionFile
{
    public string? OriginalFilePath { get; set; }
    public string? AutosaveFilePath { get; set; }

    public SessionFile(string? originalFilePath, string? autosaveFilePath)
    {
        OriginalFilePath = originalFilePath;
        AutosaveFilePath = autosaveFilePath;
    }
}
