using System.Diagnostics;

namespace PixiEditor.Common.Performance;

internal record PerfEvent(PerfEventType Type, long StartTimestampTicks, long DurationTicks = -1)
{
    public override string ToString()
    {
        double timestampMillis = StartTimestampTicks / (double)Stopwatch.Frequency * 1000;
        TimeSpan timestampTimeSpan = TimeSpan.FromMilliseconds(timestampMillis);
        if (DurationTicks != -1)
        {
            double durationMillis = DurationTicks / (double)Stopwatch.Frequency * 1000;
            return $"[{timestampTimeSpan}] {Type.ToString()} took {durationMillis:F3}ms";
        }
        
        return $"[{timestampTimeSpan}] {Type.ToString()}";
    }
}
