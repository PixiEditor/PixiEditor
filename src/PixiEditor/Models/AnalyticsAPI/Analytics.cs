using PixiEditor.Models.Files;
using PixiEditor.Numerics;

namespace PixiEditor.Models.AnalyticsAPI;

public static class Analytics
{
    public static AnalyticEvent SendStartup(StartupPerformance startup) =>
        SendEvent(AnalyticEventTypes.Startup, startup.GetData());

    public static AnalyticEvent SendCreateDocument(int width, int height) =>
        SendEvent(AnalyticEventTypes.CreateDocument, ("Width", width), ("Height", height));

    internal static AnalyticEvent SendOpenFile(IoFileType fileType, long fileSize, VecI size) =>
        SendEvent(AnalyticEventTypes.OpenFile, ("FileType", fileType.PrimaryExtension), ("FileSize", fileSize), ("Width", size.X), ("Height", size.Y));
    
    private static AnalyticEvent SendEvent(string name, params (string, object)[] data) =>
        SendEvent(name, data.ToDictionary());

    private static AnalyticEvent SendEvent(string name, Dictionary<string, object> data)
    {
        var e = new AnalyticEvent
        {
            EventType = name,
            Time = DateTimeOffset.Now,
            Data = data
        };
        
        var reporter = AnalyticsPeriodicReporter.Instance;

        reporter.AddEvent(e);

        return e;
    }
}
