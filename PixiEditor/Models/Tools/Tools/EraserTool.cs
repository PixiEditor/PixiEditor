using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.Tools.Tools
{
    public class EraserTool : BitmapOperationTool
    {
        private readonly PenTool pen = new PenTool();

        public EraserTool()
        {
            ActionDisplay = "Draw to remove color from a pixel.";
            Tooltip = "Erasers color from pixel. (E)";
            Toolbar = new BasicToolbar();
        }

        public override LayerChange[] Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            return Erase(layer, coordinates, Toolbar.GetSetting<SizeSetting>("ToolSize").Value);
        }

        public LayerChange[] Erase(Layer layer, Coordinates[] coordinates, int toolSize)
        {
            Coordinates startingCords = coordinates.Length > 1 ? coordinates[1] : coordinates[0];
            BitmapPixelChanges pixels = pen.Draw(startingCords, coordinates[0], System.Windows.Media.Colors.Transparent, toolSize);
            return Only(pixels, layer);
        }
    }
}