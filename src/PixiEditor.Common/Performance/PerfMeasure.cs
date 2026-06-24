namespace PixiEditor.Common.Performance;

public class PerfMeasure : IDisposable
{
    PerfEventType eventType;
    long startTimestampTicks;
    
    public PerfMeasure(PerfEventType eventType)
    {
        this.eventType = eventType;
        this.startTimestampTicks = PerfLogger.GetCurrentTimestamp();
    }
    
    public void Dispose()
    {
        PerfLogger.LogEventDuration(eventType, startTimestampTicks);
    }
}
