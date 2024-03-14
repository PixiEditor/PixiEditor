using System.Windows;
using System.Windows.Threading;
using PixiEditor.Models.DocumentModels.Autosave.Enums;
using PixiEditor.Views.UserControls;

namespace PixiEditor.Models.DocumentModels.Autosave;

#nullable enable
internal class AutosaverWaitJob(TimeSpan duration)
    : IAutosaverJob
{
    public event EventHandler? OnCompleted;
    private DispatcherTimer? waitingTimer;

    public AutosaveState CorrespondingState => AutosaveState.Idle;

    public void Start()
    {
        waitingTimer = new(duration, DispatcherPriority.Normal, WaitEndCallback, Application.Current.Dispatcher);
        waitingTimer.Start();
    }

    private void WaitEndCallback(object sender, EventArgs e)
    {
        waitingTimer!.Stop();
        OnCompleted?.Invoke(this, EventArgs.Empty);
    }

    public void OnUpdateableChangeEnded() { }

    public void ForceStop()
    {
        waitingTimer!.Stop();
    }
}
