using System.Diagnostics;
using System.Reflection;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.Models.Commands.CommandContext;
using PixiEditor.Models.Files;
using PixiEditor.Models.Handlers;
using Drawie.Numerics;

namespace PixiEditor.Models.AnalyticsAPI;

public static class Analytics
{
    public static AnalyticEvent? SendStartup(StartupPerformance startup) =>
        SendEvent(AnalyticEventTypes.Startup, startup.GetData());

    public static AnalyticEvent? SendCreateDocument(int width, int height) =>
        SendEvent(AnalyticEventTypes.CreateDocument, ("Width", width), ("Height", height));

    internal static AnalyticEvent? SendOpenFile(IoFileType fileType, long fileSize, VecI size) =>
        SendEvent(AnalyticEventTypes.OpenFile, ("FileType", fileType.PrimaryExtension), ("FileSize", fileSize), ("Width", size.X), ("Height", size.Y));

    internal static AnalyticEvent? SendCreateNode(Type nodeType) =>
        SendEvent(AnalyticEventTypes.CreateNode, ("NodeType", nodeType.GetCustomAttribute<NodeInfoAttribute>()?.UniqueName));

    internal static AnalyticEvent? SendCreateKeyframe(int position, string type, int fps, int duration, int totalKeyframes) =>
        SendEvent(
            AnalyticEventTypes.CreateKeyframe,
            ("Position", position),
            ("Type", type),
            ("FPS", fps),
            ("Duration", duration),
            ("TotalKeyframes", totalKeyframes));

    internal static AnalyticEvent? SendCloseDocument() => SendEvent(AnalyticEventTypes.CloseDocument);
    
    internal static AnalyticEvent? SendOpenExample(string fileName) => SendEvent(AnalyticEventTypes.OpenExample, ("FileName", fileName));

    internal static AnalyticEvent? SendUseTool(IToolHandler? tool, VecD positionOnCanvas, VecD documentSize) =>
        SendEvent(AnalyticEventTypes.UseTool, ("Tool", tool?.ToolName), ("Position", new VecD(positionOnCanvas.X / documentSize.X, positionOnCanvas.Y / documentSize.Y)));

    internal static AnalyticEvent? SendSwitchToTool(IToolHandler? newTool, IToolHandler? oldTool, ICommandExecutionSourceInfo? sourceInfo) =>
        SendEvent(AnalyticEventTypes.SwitchTool, ("NewTool", newTool?.ToolName), ("OldTool", oldTool?.ToolName), ("Source", sourceInfo));

    internal static AnalyticEvent? SendCommand(string commandName, ICommandExecutionSourceInfo? source, bool expectingEndTime = false) =>
        source is ShortcutSourceInfo { IsRepeat: true } ? null : SendEvent(AnalyticEventTypes.GeneralCommand, expectingEndTime, ("CommandName", commandName), ("Source", source));

    private static AnalyticEvent? SendEvent(string name, params (string, object)[] data) =>
        SendEvent(name, data.ToDictionary());
    
    private static AnalyticEvent? SendEvent(string name, bool expectingEndTime, params (string, object)[] data) =>
        SendEvent(name, data.ToDictionary(), expectingEndTime);

    private static AnalyticEvent? SendEvent(string name, Dictionary<string, object> data, bool expectingEndTime = false)
    {
        var reporter = AnalyticsPeriodicReporter.Instance;

        if (reporter == null)
        {
            return null;
        }

        var e = new AnalyticEvent
        {
            EventType = name,
            Time = DateTime.UtcNow,
            ExpectingEndTimeReport = expectingEndTime,
            Data = data
        };
        
        reporter.AddEvent(e);

        return e;
    }
}
