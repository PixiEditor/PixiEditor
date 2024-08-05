namespace PixiEditor.Models.AnalyticsAPI;

public class AnalyticEventTypes
{
    public string Startup { get; } = GetEventType("Startup");
    public string CreateDocument { get; } = GetEventType("CreateDocument");
    public string SwitchTab { get; } = GetEventType("SwitchTab");
    public string OpenWindow { get; } = GetEventType("OpenWindow");
    public string CreateNode { get; } = GetEventType("CreateNode");
    public string CreateKeyframe { get; } = GetEventType("CreateKeyframe");
    public string CloseDocument { get; } = GetEventType("CloseDocument");
    public string ResizeDocument { get; } = GetEventType("ResizeDocument");
    public string OpenExample { get; } = GetEventType("OpenExample");
    public string OpenFile { get; } = GetEventType("OpenFile");
    public string GeneralCommand { get; } = GetEventType("GeneralCommand");
    public string SwitchTool { get; } = GetEventType("SwitchTool");
    public string UseTool { get; } = GetEventType("UseTool");
    public string SetColor { get; } = GetEventType("SetColor");

    private static string GetEventType(string value) => $"PixiEditor.{value}";
}
