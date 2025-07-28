using Avalonia.Threading;

namespace PixiEditor.Models.DocumentModels.Autosave;

internal class AutosaverWaitJob(TimeSpan duration)
    : IAutosaverJob
{
    public event Action? OnCompleted;
    private DispatcherTimer? waitingTimer;

    public AutosaveState CorrespondingState => AutosaveState.Idle;

    public void Start()
    {
        waitingTimer = new(duration, DispatcherPriority.Normal, WaitEndCallback);
        waitingTimer.Start();
    }

    private void WaitEndCallback(object sender, EventArgs e)
    {
        waitingTimer!.Stop();
        OnCompleted?.Invoke();
    }

    public void OnUpdateableChangeEnded() { }

    public void ForceStop()
    {
        waitingTimer!.Stop();
    }
}
