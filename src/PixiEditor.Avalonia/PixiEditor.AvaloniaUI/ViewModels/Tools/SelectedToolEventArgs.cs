namespace PixiEditor.AvaloniaUI.ViewModels.Tools;

internal class SelectedToolEventArgs
{
    public SelectedToolEventArgs(ToolViewModel oldTool, ToolViewModel newTool)
    {
        OldTool = oldTool;
        NewTool = newTool;
    }

    public ToolViewModel OldTool { get; set; }

    public ToolViewModel NewTool { get; set; }
}
