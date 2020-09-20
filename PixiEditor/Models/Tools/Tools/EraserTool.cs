using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.Tools.Tools
{
    public class EraserTool : BitmapOperationTool
    {
        public override ToolType ToolType => ToolType.Eraser;

        public EraserTool()
        {
            Tooltip = "Erasers color from pixel (E)";
            Toolbar = new BasicToolbar();
        }

        public override LayerChange[] Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            return Erase(layer, coordinates, Toolbar.GetSetting<int>("ToolSize").Value);
        }

        public LayerChange[] Erase(Layer layer, Coordinates[] coordinates, int toolSize)
        {
            Coordinates startingCords = coordinates.Length > 1 ? coordinates[1] : coordinates[0];
            PenTool pen = new PenTool();
            var pixels = pen.Draw(startingCords, coordinates[0], System.Windows.Media.Colors.Transparent, toolSize);
            return new[] {new LayerChange(pixels, layer)};
        }
    }
}