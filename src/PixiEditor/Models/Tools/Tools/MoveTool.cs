using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Commands.Attributes.Commands;

namespace PixiEditor.Models.Tools.Tools;

[Command.Tool(Key = Key.V)]
internal class MoveTool : BitmapOperationTool
{
    private string defaultActionDisplay = "Hold mouse to move selected pixels. Hold Ctrl to move all layers.";

    public MoveTool()
    {
        ActionDisplay = defaultActionDisplay;
        Cursor = Cursors.Arrow;
    }

    public override string Tooltip => $"Moves selected pixels ({Shortcut}). Hold Ctrl to move all layers.";

    public override bool HideHighlight => true;

    public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (ctrlIsDown)
            ActionDisplay = "Hold mouse to move all layers.";
        else
            ActionDisplay = defaultActionDisplay;
    }

    public override void Use()
    {

    }
}
