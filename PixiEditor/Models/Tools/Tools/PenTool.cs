using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.Tools.Tools
{
    public class PenTool : BitmapOperationTool
    {
        private readonly SizeSetting toolSizeSetting;

        public PenTool()
        {
            Cursor = Cursors.Pen;
            Tooltip = "Standard brush (B)";
            Toolbar = new BasicToolbar();
            toolSizeSetting = Toolbar.GetSetting<SizeSetting>("ToolSize");
        }

        public override ToolType ToolType => ToolType.Pen;

        public override LayerChange[] Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            var startingCords = coordinates.Length > 1 ? coordinates[1] : coordinates[0];
            var pixels = Draw(startingCords, coordinates[0], color, toolSizeSetting.Value);
            return Only(pixels, layer);
        }

        public BitmapPixelChanges Draw(Coordinates startingCoords, Coordinates latestCords, Color color, int toolSize)
        {
            var line = new LineTool();
            return BitmapPixelChanges.FromSingleColoredArray(
                line.CreateLine(startingCoords, latestCords, toolSize), color);
        }
    }
}