using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes;

namespace PixiEditor.Models.Tools.Tools;

[Command.Tool(Key = Key.R)]
public class RectangleTool : ShapeTool
{
    private string defaultActionDisplay = "Click and move to draw a rectangle. Hold Shift to draw a square.";
    public RectangleTool()
    {
        ActionDisplay = defaultActionDisplay;
    }

    public override string Tooltip => $"Draws rectangle on canvas ({Shortcut}). Hold Shift to draw a square.";

    public bool Filled { get; set; } = false;

    public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (shiftIsDown)
            ActionDisplay = "Click and move to draw a square.";
        else
            ActionDisplay = defaultActionDisplay;
    }

    public override void Use()
    {
    }
}
