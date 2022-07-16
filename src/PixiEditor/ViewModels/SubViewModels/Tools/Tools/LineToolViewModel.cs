using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.L)]
internal class LineToolViewModel : ShapeTool
{
    private string defaultActionDisplay = "Click and move to draw a line. Hold Shift to draw an even one.";

    public LineToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
        Toolbar = new BasicToolbar();
    }

    public override string Tooltip => $"Draws line on canvas ({Shortcut}). Hold Shift to draw even line.";

    public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (shiftIsDown)
            ActionDisplay = "Click and move mouse to draw an even line.";
        else
            ActionDisplay = defaultActionDisplay;
    }
}
