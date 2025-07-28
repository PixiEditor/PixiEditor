namespace PixiEditor.Models.AnalyticsAPI;

public class AnalyticEvent
{
    private Semaphore? _endTimeReportedSemaphore;
    
    public string EventType { get; set; }
    
    public DateTime Time { get; set; }
    
    public DateTime End { get; set; }
    
    public bool ExpectingEndTimeReport { get; set; }

    public void ReportEndTime()
    {
        End = DateTime.UtcNow;
        ExpectingEndTimeReport = false;
        
        _endTimeReportedSemaphore?.Release();
    }

    public void WaitForEndTime(TimeSpan timeout)
    {
        _endTimeReportedSemaphore = new Semaphore(0, int.MaxValue);

        _endTimeReportedSemaphore.WaitOne(timeout);
        _endTimeReportedSemaphore?.Dispose();
        _endTimeReportedSemaphore = null;
    }
    
    public Dictionary<string, object>? Data { get; set; }
}
