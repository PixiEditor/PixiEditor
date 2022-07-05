using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes;

namespace PixiEditor.Models.Tools.Tools;

[Command.Tool(Key = Key.C)]
internal class CircleTool : ShapeTool
{
    private string defaultActionDisplay = "Click and move mouse to draw a circle. Hold Shift to draw an even one.";

    public CircleTool()
    {
        ActionDisplay = defaultActionDisplay;
    }

    public override string Tooltip => $"Draws circle on canvas ({Shortcut}). Hold Shift to draw even circle.";

    public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (shiftIsDown)
            ActionDisplay = "Click and move mouse to draw an even circle.";
        else
            ActionDisplay = defaultActionDisplay;
    }

    public override void Use()
    {

    }
}
