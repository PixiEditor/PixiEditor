using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.Tools.Tools;

[Command.Tool(Key = Key.E)]
internal class EraserTool : BitmapOperationTool
{
    public EraserTool()
    {
        ActionDisplay = "Draw to remove color from a pixel.";
        Toolbar = new BasicToolbar();
    }

    public override string Tooltip => $"Erasers color from pixel. ({Shortcut})";

    public override void Use()
    {

    }
}
