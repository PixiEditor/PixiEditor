namespace PixiEditor.Models.DocumentModels.Autosave;

public struct AutosaveSaveData
{
    public LastBackupAutosaveData? LastBackupAutosaveData { get; set; }
    public LastUserFileAutosaveData? LastUserFileAutosaveData { get; set; }
    public AutosaveState AutosaveState { get; set; }
    public DateTime AutosaveLaunchDateTime { get; set; }
    public TimeSpan AutosaveInterval { get; set; }
}

public struct LastBackupAutosaveData
{
    public DateTime Time { get; set; }
    public BackupAutosaveResult SaveResult { get; set; }
}
