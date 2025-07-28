using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PixiEditor.Models.AnalyticsAPI;

public class StartupPerformance
{
    private readonly DateTimeOffset _processStart;
    
    private TimeSpan _timeToMainWindow;
    private TimeSpan _timeToMainViewModel;
    private TimeSpan _timeToInteractivity;

    // Do not rename - Used in GetKeyValue
    public DateTimeOffset ProcessStart => _processStart;

    // Do not rename - Used in GetKeyValue
    public TimeSpan TimeToMainWindow => _timeToMainWindow;

    // Do not rename - Used in GetKeyValue
    public TimeSpan TimeToMainViewModel => _timeToMainViewModel;

    // Do not rename - Used in GetKeyValue
    public TimeSpan TimeToInteractivity => _timeToInteractivity;

    public StartupPerformance()
    {
        _processStart = Process.GetCurrentProcess().StartTime;
    }

    public void ReportToMainWindow() => ReportFor(out _timeToMainWindow);
    
    public void ReportToMainViewModel() => ReportFor(out _timeToMainViewModel);

    public void ReportToInteractivity() => ReportFor(out _timeToInteractivity);

    public Dictionary<string, object> GetData() => new[]
    {
        GetKeyValue(ProcessStart),
        GetKeyValue(TimeToMainWindow),
        GetKeyValue(TimeToMainViewModel),
        GetKeyValue(TimeToInteractivity)
    }.ToDictionary();
    
    private void ReportFor(out TimeSpan t) => t = DateTimeOffset.Now - _processStart;

    private static KeyValuePair<string, object> GetKeyValue(object value, [CallerArgumentExpression(nameof(value))] string name = "") =>
        new(name, value);
}
