namespace PixiEditor.Models.DocumentModels.Autosave;

public enum AutosaveState
{
    Paused,
    Idle,
    AwaitingUpdateableChangeEnd,
    InProgress
}
