using System.Diagnostics;
using Timer = System.Timers.Timer;

namespace PixiEditor.Models.AnalyticsAPI;

public class PeriodicPerformanceReporter(AnalyticsPeriodicReporter analyticsReporter)
{
    private long _lastTotalAllocatedBytes;
    private TimeSpan _lastTotalGcPauseTime;

    private int targetCount;
    private int sendCount;
    
    private Timer timer;
    
    public void StartPeriodicReporting()
    {
        var recordInterval = TimeSpan.FromSeconds(45);
        var targetRecordingSpan = TimeSpan.FromHours(1);

        targetCount = (int)(targetRecordingSpan / recordInterval);

        timer = new Timer
        {
            Interval = recordInterval.TotalMilliseconds,
            AutoReset = true
        };

        timer.Elapsed += (_, _) => Task.Run(CollectPerformanceMetricAsync);
        
        timer.Start();
    }
    
    private async Task CollectPerformanceMetricAsync()
    {
        var process = Process.GetCurrentProcess();
        
        var data = new Dictionary<string, object>();

        var collectionStartTime = DateTime.Now;
        
        var processorTime = await SampleProcessorTimeAsync(process);

        data["UserTime"] = processorTime.userTime;
        data["PrivilegedTime"] = processorTime.privilegedTime;

        data["PrivateMemorySize"] = process.PrivateMemorySize64;
        data["WorkingSet"] = process.WorkingSet64;

        var currentTotalGcPauseTime = GC.GetTotalPauseDuration();
        data["GcPauseTime"] = _lastTotalGcPauseTime;
        _lastTotalGcPauseTime = currentTotalGcPauseTime;

        var handles = process.HandleCount;
        var threads = process.Threads.Count;
        
        data["Handles"] = handles;
        data["Threads"] = threads;
        
        data["CollectionTime"] = DateTime.Now - collectionStartTime;
        
        var e = new AnalyticEvent
        {
            EventType = AnalyticEventTypes.PeriodicPerformanceReport,
            Time = DateTime.UtcNow,
            Data = data
        };
        
        analyticsReporter.AddEvent(e);
        
        sendCount++;

        if (sendCount >= targetCount)
        {
            timer.Stop();
            timer.Dispose();
        }
    }

    private async Task<(TimeSpan userTime, TimeSpan privilegedTime)> SampleProcessorTimeAsync(Process process)
    {
        var userStartTime = process.UserProcessorTime;
        var privilegedStartTime = process.PrivilegedProcessorTime;

        await Task.Delay(TimeSpan.FromSeconds(10));
        
        process.Refresh();
        var userTime = process.UserProcessorTime - userStartTime;
        var privilegedTime = process.PrivilegedProcessorTime - privilegedStartTime;
        
        return (userTime, privilegedTime);
    }
}
