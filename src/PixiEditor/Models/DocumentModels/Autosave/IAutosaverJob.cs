namespace PixiEditor.Models.DocumentModels.Autosave;

internal interface IAutosaverJob
{
    event Action OnCompleted;
    AutosaveState CorrespondingState { get; }
    void OnUpdateableChangeEnded();
    void Start();
    void ForceStop();
}
