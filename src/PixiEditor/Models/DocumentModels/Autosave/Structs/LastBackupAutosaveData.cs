using PixiEditor.Models.DocumentModels.Autosave.Enums;

namespace PixiEditor.Models.DocumentModels.Autosave.Structs;

public struct LastBackupAutosaveData
{
    public DateTime Time { get; set; }
    public BackupAutosaveResult SaveResult { get; set; }
}
