using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Tools;

internal class SelectedToolEventArgs
{
    public SelectedToolEventArgs(IToolHandler oldTool, IToolHandler newTool)
    {
        OldTool = oldTool;
        NewTool = newTool;
    }

    public IToolHandler OldTool { get; set; }

    public IToolHandler NewTool { get; set; }
}
