using PixiEditor.Models.Controllers;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using SkiaSharp;
using System.Collections.Generic;

namespace PixiEditor.Models.Tools.Tools
{
    public class EraserTool : BitmapOperationTool
    {
        private readonly PenTool pen;

        public EraserTool(BitmapManager bitmapManager)
        {
            ActionDisplay = "Draw to remove color from a pixel.";
            Toolbar = new BasicToolbar();
            pen = new PenTool(bitmapManager);
        }

        public override bool UsesShift => false;

        public override string Tooltip => "Erasers color from pixel. (E)";

        public override void Use(Layer layer, List<Coordinates> coordinates, SKColor color)
        {
            Erase(layer, coordinates, Toolbar.GetSetting<SizeSetting>("ToolSize").Value);
        }

        public void Erase(Layer layer, List<Coordinates> coordinates, int toolSize)
        {
            Coordinates startingCords = coordinates.Count > 1 ? coordinates[1] : coordinates[0];
            pen.Draw(layer, startingCords, coordinates[0], SKColors.Transparent, toolSize, false, null, SKBlendMode.Src);
        }
    }
}
