using Avalonia.Threading;

namespace PixiEditor.Engine.AvaloniaPlatform;

public class PixiEngineDispatcher : IDispatcherImpl
{
    private readonly Thread _mainThread;
    private readonly SysTimer _timer;
    private readonly SendOrPostCallback _invokeSignaled; // cached delegate
    private readonly SendOrPostCallback _invokeTimer;  // cached delegate
    
    public void Signal()
    {
        throw new NotImplementedException();
    }

    public void UpdateTimer(long? dueTimeInMs)
    {
        throw new NotImplementedException();
    }

    public bool CurrentThreadIsLoopThread => _mainThread == Thread.CurrentThread;
    public long Now { get; }
    public event Action? Signaled;
    public event Action? Timer;
}
