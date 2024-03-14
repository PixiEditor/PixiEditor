#nullable enable
namespace PixiEditor.Models.DocumentModels.Autosave.Enums;

public enum BackupAutosaveResult
{
    Success,
    Error,
    NothingToSave,
    BlockedByUpdateableChange
}
