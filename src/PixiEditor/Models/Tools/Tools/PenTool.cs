using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.Tools.Tools;

[Command.Tool(Key = Key.B)]
internal class PenTool : ShapeTool
{
    private readonly SizeSetting toolSizeSetting;
    private readonly BoolSetting pixelPerfectSetting;

    public PenTool(BitmapManager bitmapManager)
    {
        Cursor = Cursors.Pen;
        ActionDisplay = "Click and move to draw.";
        Toolbar = new PenToolbar();
        toolSizeSetting = Toolbar.GetSetting<SizeSetting>("ToolSize");
        pixelPerfectSetting = Toolbar.GetSetting<BoolSetting>("PixelPerfectEnabled");
    }

    public override string Tooltip => $"Standard brush. ({Shortcut})";

    public override void Use()
    {
    }
}
