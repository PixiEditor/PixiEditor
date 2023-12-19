using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PixiEditor.Models.Controllers;

#nullable enable
internal class MouseUpdateControllerSession : IDisposable
{
    private const double IntervalMs = 1000 / 142.0; //142 Hz

    private readonly Action onStartListening;
    private readonly Action onStopListening;
    private readonly MouseEventHandler onMouseMove;
    
    private readonly AutoResetEvent resetEvent = new(false);
    private readonly object lockObj = new();

    /// <summary>
    /// <see cref="MouseUpdateControllerSession"/> doesn't rely on attaching and detaching mouse move handler,
    /// it just ignores mouse move events when not listening. <br/>
    /// Yet it still calls <see cref="onStartListening"/> and <see cref="onStopListening"/> which can be used to attach and detach event handler elsewhere.
    /// </summary>
    private bool isListening = true;
    private bool isDisposed = false;

    public MouseUpdateControllerSession(Action onStartListening, Action onStopListening, MouseEventHandler onMouseMove)
    {
        this.onStartListening = onStartListening;
        this.onStopListening = onStopListening;
        this.onMouseMove = onMouseMove;

        Thread timerThread = new(TimerLoop);
        timerThread.Name = "MouseUpdateController thread";
        timerThread.Start();

        onStartListening();
    }

    public void MouseMoveInput(object sender, MouseEventArgs e)
    {
        if (!isListening || isDisposed)
            return;
        
        bool lockWasTaken = false;
        try
        {
            Monitor.TryEnter(lockObj, ref lockWasTaken);
            if (lockWasTaken)
            {
                isListening = false;
                onStopListening();
                onMouseMove(sender, e);
                resetEvent.Set();
            }
        }
        finally
        {
            if (lockWasTaken)
                Monitor.Exit(lockObj);
        }
    }
    
    public void Dispose()
    {
        isDisposed = true;
        resetEvent.Dispose();
    }

    private void TimerLoop()
    {
        try
        {
            long lastThreadIter = Stopwatch.GetTimestamp();
            while (!isDisposed)
            {
                // call waitOne periodically instead of waiting infinitely to make sure we crash or exit when resetEvent is disposed
                if (!resetEvent.WaitOne(300))
                {
                    lastThreadIter = Stopwatch.GetTimestamp();
                    continue;
                }

                lock (lockObj)
                {
                    double sleepDur = Math.Clamp(IntervalMs - Stopwatch.GetElapsedTime(lastThreadIter).TotalMilliseconds, 0, IntervalMs);
                    lastThreadIter += (long)(IntervalMs * Stopwatch.Frequency / 1000);
                    if (sleepDur > 0)
                        Thread.Sleep((int)Math.Round(sleepDur));
                    
                    if (isDisposed)
                        return;

                    isListening = true;
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        if (!isDisposed)
                            onStartListening();
                    });
                }
            }
        }
        catch (ObjectDisposedException)
        {
            return;
        }
        catch (Exception e)
        {
            Application.Current?.Dispatcher.BeginInvoke(() => throw new AggregateException("Input handling thread died", e), DispatcherPriority.SystemIdle);
            throw;
        }
    }
}
