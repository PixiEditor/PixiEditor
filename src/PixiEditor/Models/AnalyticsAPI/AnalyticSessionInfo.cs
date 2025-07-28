using PixiEditor.OperatingSystem;

namespace PixiEditor.Models.AnalyticsAPI;

public class AnalyticSessionInfo(IOperatingSystem os)
{
    public Version Version { get; set; }

    public string BuildId { get; set; }

    public string? PlatformId { get; set; } = os.AnalyticsId;

    public string? PlatformName { get; set; } = os.AnalyticsName;
}
