using System.Collections.Generic;
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
        private PenTool pen = new PenTool();

        public EraserTool()
        {
            ActionDisplay = "Draw to remove color from a pixel.";
            Tooltip = "Erasers color from pixel. (E)";
            Toolbar = new BasicToolbar();
        }

        public override LayerChange[] Use(Layer layer, List<Coordinates> coordinates, Color color)
        {
            return Erase(layer, coordinates, Toolbar.GetSetting<SizeSetting>("ToolSize").Value);
        }

        public LayerChange[] Erase(Layer layer, List<Coordinates> coordinates, int toolSize)
        {
            Coordinates startingCords = coordinates.Count > 1 ? coordinates[1] : coordinates[0];
            BitmapPixelChanges pixels = pen.Draw(startingCords, coordinates[0], System.Windows.Media.Colors.Transparent, toolSize);
            return Only(pixels, layer);
        }

        public override void SetupSubTools()
        {
            pen = CreateSubTool<PenTool>();
        }
    }
}