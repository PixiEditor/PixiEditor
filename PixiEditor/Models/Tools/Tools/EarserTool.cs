using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings;

namespace PixiEditor.Models.Tools.Tools
{
    public class EarserTool : BitmapOperationTool
    {
        public override ToolType ToolType => ToolType.Earser;

        public EarserTool()
        {
            Tooltip = "Earsers color from pixel (E)";
            Toolbar = new BasicToolbar();
        }

        public override LayerChange[] Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            return Earse(layer, coordinates, (int) Toolbar.GetSetting("ToolSize").Value);
        }

        public LayerChange[] Earse(Layer layer, Coordinates[] coordinates, int toolSize)
        {
            Coordinates startingCords = coordinates.Length > 1 ? coordinates[1] : coordinates[0];
            PenTool pen = new PenTool();
            var pixels = pen.Draw(startingCords, coordinates[0], System.Windows.Media.Colors.Transparent, toolSize);
            return new[] {new LayerChange(pixels, layer)};
        }
    }
}