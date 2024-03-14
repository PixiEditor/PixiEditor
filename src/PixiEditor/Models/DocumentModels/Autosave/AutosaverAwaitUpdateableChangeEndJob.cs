using PixiEditor.Models.DocumentModels.Autosave.Enums;
using PixiEditor.Views.UserControls;

namespace PixiEditor.Models.DocumentModels.Autosave;

#nullable enable
internal class AutosaverAwaitUpdateableChangeEndJob : IAutosaverJob
{
    public event EventHandler? OnCompleted;
    private bool isStopped = true;

    public AutosaveState CorrespondingState => AutosaveState.AwaitingUpdateableChangeEnd;

    public void OnUpdateableChangeEnded()
    {
        if (isStopped)
            return;
        OnCompleted?.Invoke(this, EventArgs.Empty);
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
