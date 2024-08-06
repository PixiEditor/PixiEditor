using System.Reflection;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.Models.Commands.CommandContext;
using PixiEditor.Models.Files;
using PixiEditor.Models.Handlers;
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

    internal static AnalyticEvent SendCreateNode(Type nodeType) =>
        SendEvent(AnalyticEventTypes.CreateNode, ("NodeType", nodeType.GetCustomAttribute<NodeInfoAttribute>()?.UniqueName));

    internal static AnalyticEvent SendCreateKeyframe(int position, string type, int fps, int duration, int totalKeyframes) =>
        SendEvent(
            AnalyticEventTypes.CreateKeyframe,
            ("Position", position),
            ("Type", type),
            ("FPS", fps),
            ("Duration", duration),
            ("TotalKeyframes", totalKeyframes));

    internal static AnalyticEvent SendCloseDocument() => SendEvent(AnalyticEventTypes.CloseDocument);
    
    internal static AnalyticEvent SendOpenExample(string fileName) => SendEvent(AnalyticEventTypes.OpenExample, ("FileName", fileName));

    internal static AnalyticEvent SendUseTool(IToolHandler? tool, VecD positionOnCanvas, VecD documentSize) =>
        SendEvent(AnalyticEventTypes.UseTool, ("Tool", tool?.ToolName), ("Position", new VecD(documentSize.X / positionOnCanvas.X, documentSize.Y / positionOnCanvas.Y)));

    internal static AnalyticEvent SendSwitchToTool(IToolHandler? newTool, IToolHandler? oldTool, ICommandExecutionSourceInfo? sourceInfo) =>
        SendEvent(AnalyticEventTypes.SwitchTool, ("NewTool", newTool?.ToolName), ("OldTool", oldTool?.ToolName), ("Source", sourceInfo));

    internal static AnalyticEvent SendCommand(string commandName, ICommandExecutionSourceInfo? source) =>
        SendEvent(AnalyticEventTypes.GeneralCommand, ("CommandName", commandName), ("Source", source));
    
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
