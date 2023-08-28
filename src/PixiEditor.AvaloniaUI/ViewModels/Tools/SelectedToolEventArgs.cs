using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.Tools;

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
