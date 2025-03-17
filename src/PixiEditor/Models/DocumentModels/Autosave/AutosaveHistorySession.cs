namespace PixiEditor.Models.DocumentModels.Autosave;

internal class AutosaveHistorySession(Guid sessionGuid, DateTime launchDateTime)
{
    public List<AutosaveHistoryEntry> AutosaveEntries { get; set; } = new();
    public Guid SessionGuid { get; set; } = sessionGuid;
    public DateTime LaunchDateTime { get; set; } = launchDateTime;
}
