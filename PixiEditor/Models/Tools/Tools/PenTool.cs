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
        public override ToolType ToolType => ToolType.Pen;
        private readonly SizeSetting _toolSizeSetting;

        public PenTool()
        {
            Cursor = Cursors.Pen;
            Tooltip = "Standard brush (B)";
            Toolbar = new BasicToolbar();
            _toolSizeSetting = Toolbar.GetSetting<SizeSetting>("ToolSize");
        }

        public override LayerChange[] Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            Coordinates startingCords = coordinates.Length > 1 ? coordinates[1] : coordinates[0];
            var pixels = Draw(startingCords, coordinates[0], color, _toolSizeSetting.Value);
            return Only(pixels, layer);
        }

        public BitmapPixelChanges Draw(Coordinates startingCoords, Coordinates latestCords, Color color, int toolSize)
        {
            LineTool line = new LineTool();
            return BitmapPixelChanges.FromSingleColoredArray(
                line.CreateLine(startingCoords, latestCords, toolSize), color);
        }
    }
}