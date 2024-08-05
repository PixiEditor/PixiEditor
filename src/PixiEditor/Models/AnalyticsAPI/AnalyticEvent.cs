namespace PixiEditor.Models.AnalyticsAPI;

public class AnalyticEvent
{
    public string EventType { get; set; }
    
    public DateTimeOffset Time { get; set; }
    
    public DateTimeOffset End { get; set; }
    
    public Dictionary<string, object>? Data { get; set; }
}
