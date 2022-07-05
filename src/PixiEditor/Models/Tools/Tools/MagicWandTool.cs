using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.Tools.Tools;

[Command.Tool(Key = Key.W)]
internal class MagicWandTool : ReadonlyTool
{
    public override string Tooltip => $"Magic Wand ({Shortcut}). Flood's the selection";

    public MagicWandTool()
    {
        Toolbar = new MagicWandToolbar();
        ActionDisplay = "Click to flood the selection.";
    }

    public override void Use(VecD position)
    {

    }
}
