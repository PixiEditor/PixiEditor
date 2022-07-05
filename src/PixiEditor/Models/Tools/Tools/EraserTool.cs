using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.Tools.Tools;

[Command.Tool(Key = Key.E)]
internal class EraserTool : BitmapOperationTool
{
    private readonly PenTool pen;

    public EraserTool(BitmapManager bitmapManager)
    {
        ActionDisplay = "Draw to remove color from a pixel.";
        Toolbar = new BasicToolbar();
        pen = new PenTool(bitmapManager);
    }

    public override string Tooltip => $"Erasers color from pixel. ({Shortcut})";

    public override void Use()
    {

    }
}
