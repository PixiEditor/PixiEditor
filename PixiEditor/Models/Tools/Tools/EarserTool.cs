using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings;
using System.Windows.Media;

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
            PenTool pen = new PenTool();
            var pixels = pen.Draw(coordinates[0], System.Windows.Media.Colors.Transparent, (int)Toolbar.GetSetting("ToolSize").Value);
            return new LayerChange[] { new LayerChange(pixels, layer) };
        }
    }
}
