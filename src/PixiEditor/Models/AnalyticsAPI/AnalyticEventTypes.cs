namespace PixiEditor.Models.AnalyticsAPI;

public class AnalyticEventTypes
{
    public static string Startup { get; } = GetEventType("Startup");
    public static string CreateDocument { get; } = GetEventType("CreateDocument");
    public static string SwitchTab { get; } = GetEventType("SwitchTab");
    public static string OpenWindow { get; } = GetEventType("OpenWindow");
    public static string CreateNode { get; } = GetEventType("CreateNode");
    public static string CreateKeyframe { get; } = GetEventType("CreateKeyframe");
    public static string CloseDocument { get; } = GetEventType("CloseDocument");
    public static string ResizeDocument { get; } = GetEventType("ResizeDocument");
    public static string OpenExample { get; } = GetEventType("OpenExample");
    public static string OpenFile { get; } = GetEventType("OpenFile");
    public static string GeneralCommand { get; } = GetEventType("GeneralCommand");
    public static string SwitchTool { get; } = GetEventType("SwitchTool");
    public static string UseTool { get; } = GetEventType("UseTool");
    public static string SetColor { get; } = GetEventType("SetColor");

    private static string GetEventType(string value) => $"PixiEditor.{value}";
}
