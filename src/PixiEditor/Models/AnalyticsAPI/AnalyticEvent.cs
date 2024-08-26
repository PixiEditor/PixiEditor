namespace PixiEditor.Models.AnalyticsAPI;

public class AnalyticEvent
{
    public string EventType { get; set; }
    
    public DateTime Time { get; set; }
    
    public DateTime End { get; set; }
    
    public Dictionary<string, object>? Data { get; set; }
}
