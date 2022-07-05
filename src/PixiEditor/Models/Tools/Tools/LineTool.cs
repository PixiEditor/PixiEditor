using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.Tools.Tools;

[Command.Tool(Key = Key.L)]
public class LineTool : ShapeTool
{
    private string defaltActionDisplay = "Click and move to draw a line. Hold Shift to draw an even one.";

    public LineTool()
    {
        ActionDisplay = defaltActionDisplay;
        Toolbar = new BasicToolbar();
    }

    public override string Tooltip => $"Draws line on canvas ({Shortcut}). Hold Shift to draw even line.";

    public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (shiftIsDown)
            ActionDisplay = "Click and move mouse to draw an even line.";
        else
            ActionDisplay = defaltActionDisplay;
    }

    public override void Use()
    {
        throw new NotImplementedException();
    }
}
