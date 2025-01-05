namespace PixiEditor.Models.AnalyticsAPI;

public class ApiCrashReport
{
    public DateTime ProcessStart { get; set; }

    public DateTime ReportTime { get; set; }

    public Guid? SessionId { get; set; }

    public bool IsCrash { get; set; }

    public string CatchLocation { get; set; }

    public string CatchMethod { get; set; }

    public Version Version { get; set; }

    public string BuildId { get; set; }

    public Dictionary<string, object> SystemInformation { get; set; } = [];

    public Dictionary<string, object> StateInformation { get; set; } = [];

    public ExceptionDetails Exception { get; set; }
}
