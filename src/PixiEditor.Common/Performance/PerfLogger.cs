using System.Diagnostics;

namespace PixiEditor.Common.Performance;

public static class PerfLogger
{
    private static Stopwatch uptimeStopwatch = new();
    private static Thread? perfLoggerThread;
    private static bool enabled = true;
    private static List<PerfEvent> events = new();
    
    private static string? filePath;
    private static StreamWriter? logFileStream;
    
    private static Timer? logFlushTimer;
    private static object eventsLocker = new();
    private static object flushLocker = new();
    
    public static void RecordStartup()
    {
        uptimeStopwatch.Start();
    }
    
    public static void Initialize(bool loggingEnabled, string? header = null, string? logFilePath = null)
    {
        enabled = loggingEnabled;
        filePath = logFilePath;
        
        if (!loggingEnabled)
        {
            events.Clear();
            return;
        }

        var directory = Path.GetDirectoryName(logFilePath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
            
        var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
        logFileStream = new StreamWriter(fileStream);
        logFileStream.WriteLine(header);
        logFlushTimer = new Timer(FlushLog, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));
    }

    private static void FlushLog(object? state)
    {
        // if FlushLog is still running from the previous tick, skip a tick  
        if (!Monitor.TryEnter(flushLocker)) return;
        try
        {
            List<PerfEvent> eventsToFlush;
            lock (eventsLocker)
            {
                if (events.Count == 0)
                    return;
                eventsToFlush = events;
                events = new List<PerfEvent>();
            }

            foreach (var e in eventsToFlush)
            {
                logFileStream.WriteLine(e.ToString());
            }
            logFileStream.Flush();
        }
        finally
        {
            Monitor.Exit(flushLocker);
        }
    }

    public static void LogEvent(PerfEventType perfEventType)
    {
        if (!enabled)
            return;
        
        lock (eventsLocker) 
        {
            events.Add(new PerfEvent(perfEventType, uptimeStopwatch.ElapsedTicks));
        }
    }

    public static void LogEventDuration(PerfEventType perfEventType, long startStopwatchTicks)
    {
        if (!enabled)
            return;

        lock (eventsLocker)
        {
            long curTicks = uptimeStopwatch.ElapsedTicks;
            events.Add(new PerfEvent(perfEventType, curTicks, curTicks - startStopwatchTicks));
        }
    }

    public static long GetCurrentTimestamp()
    {
        return uptimeStopwatch.ElapsedTicks;
    }
}
