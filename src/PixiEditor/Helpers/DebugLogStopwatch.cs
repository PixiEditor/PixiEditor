using System.Diagnostics;

namespace PixiEditor.Helpers;

/// <summary>
/// A DEBUG build only stopwatch than can log to <see cref="Debug"/>.Write()
/// </summary>
public class DebugLogStopwatch
{
    public Stopwatch Stopwatch { get; }
    
    private DebugLogStopwatch()
    {
        Stopwatch = new Stopwatch();
    }

    /// <summary>
    /// Sets the reference to <param name="stopwatch"/> a new instance of <see cref="DebugLogStopwatch"/> only in a DEBUG build
    /// </summary>
    /// <param name="stopwatch"></param>
    [Conditional("DEBUG")]
    public static void Create(ref DebugLogStopwatch? stopwatch)
    {
        stopwatch = new DebugLogStopwatch();
    }
}

internal static class DebugLogStopwatchExtensions
{
    /// <summary>
    /// Starts the Stopwatch if not null
    /// </summary>
    [Conditional("DEBUG")]
    public static void Start(this DebugLogStopwatch? stopwatch) => stopwatch?.Stopwatch.Start();

    /// <summary>
    /// Restarts the Stopwatch if not null
    /// </summary>
    [Conditional("DEBUG")]
    public static void Restart(this DebugLogStopwatch? stopwatch) => stopwatch?.Stopwatch.Restart();
    
    /// <summary>
    /// Stops the Stopwatch if not null
    /// </summary>
    [Conditional("DEBUG")]
    public static void Stop(this DebugLogStopwatch? stopwatch) => stopwatch?.Stopwatch.Stop();

    /// <summary>
    /// Logs the time using the <param name="format"/> and stops the Stopwatch if not null
    /// </summary>
    [Conditional("DEBUG")]
    public static void LogAndStop(this DebugLogStopwatch? stopwatch, string format)
    {
        if (stopwatch == null)
        {
            return;
        }

        stopwatch.Log(format);
        stopwatch.Stopwatch.Stop();
    }
    
    /// <summary>
    /// Logs the time using the <param name="format"/> if the stopwatch is not null
    /// </summary>
    [Conditional("DEBUG")]
    public static void Log(this DebugLogStopwatch? stopwatch, string format) => Debug.WriteLine(format, stopwatch?.Stopwatch.Elapsed);
}
