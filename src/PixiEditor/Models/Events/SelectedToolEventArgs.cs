using PixiEditor.ViewModels.SubViewModels.Tools;

namespace PixiEditor.Models.Events;

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
