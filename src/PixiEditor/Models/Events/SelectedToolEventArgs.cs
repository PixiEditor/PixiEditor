using PixiEditor.Models.Tools;

namespace PixiEditor.Models.Events;

internal class SelectedToolEventArgs
{
    public SelectedToolEventArgs(Tool oldTool, Tool newTool)
    {
        OldTool = oldTool;
        NewTool = newTool;
    }

    public Tool OldTool { get; set; }

    public Tool NewTool { get; set; }
}
