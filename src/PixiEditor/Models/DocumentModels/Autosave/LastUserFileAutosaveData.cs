namespace PixiEditor.Models.DocumentModels.Autosave;

public struct LastUserFileAutosaveData
{
    public DateTime Time { get; set; }
    public UserFileAutosaveResult SaveResult { get; set; }
}
