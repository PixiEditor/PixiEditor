using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using SkiaSharp;
using System.Windows.Input;

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

    public override void Use(Layer activeLayer, Layer previewLayer, IEnumerable<Layer> allLayers, IReadOnlyList<Coordinates> recordedMouseMovement, SKColor color)
    {
        Erase(activeLayer, recordedMouseMovement, Toolbar.GetSetting<SizeSetting>("ToolSize").Value);
    }

    public void Erase(Layer layer, IReadOnlyList<Coordinates> coordinates, int toolSize)
    {
        Coordinates startingCords = coordinates.Count > 1 ? coordinates[^2] : coordinates[0];
        pen.Draw(layer, startingCords, coordinates[^1], SKColors.Transparent, toolSize, false, null, SKBlendMode.Src);
    }
}