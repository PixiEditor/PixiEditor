namespace PixiEditor.Models.DocumentModels.Autosave;

public enum BackupAutosaveResult
{
    Success,
    Error,
    NothingToSave,
    BlockedByUpdateableChange
}
