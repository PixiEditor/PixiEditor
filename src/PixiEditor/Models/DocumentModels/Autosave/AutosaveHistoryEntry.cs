namespace PixiEditor.Models.DocumentModels.Autosave;

internal class AutosaveHistoryEntry(DateTime dateTime, AutosaveHistoryType type, AutosaveHistoryResult result, Guid tempFileGuid, string? originalPath)
{
    public DateTime DateTime { get; set; } = dateTime;
    public AutosaveHistoryType Type { get; set; } = type;
    public AutosaveHistoryResult Result { get; set; } = result;
    public Guid TempFileGuid { get; set; } = tempFileGuid;
    public string? OriginalPath { get; set; } = originalPath;
}
