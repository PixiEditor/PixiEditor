namespace PixiEditor.Models.DocumentModels.Autosave;

internal class AutosaverAwaitUpdateableChangeEndJob : IAutosaverJob
{
    public event Action? OnCompleted;
    private bool isStopped = true;

    public AutosaveState CorrespondingState => AutosaveState.AwaitingUpdateableChangeEnd;

    public void OnUpdateableChangeEnded()
    {
        if (isStopped)
            return;
        OnCompleted?.Invoke();
        isStopped = true;
    }

    public void Start()
    {
        isStopped = false;
    }

    public void ForceStop()
    {
        isStopped = true;
    }
}
