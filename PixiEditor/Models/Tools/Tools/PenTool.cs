using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings;

namespace PixiEditor.Models.Tools.Tools
{
    public class PenTool : BitmapOperationTool
    {
        public override ToolType ToolType => ToolType.Pen;
        private readonly int _toolSizeIndex;

        public PenTool()
        {
            Cursor = Cursors.Pen;
            Tooltip = "Standard brush (B)";
            Toolbar = new BasicToolbar();
            _toolSizeIndex = Toolbar.Settings.IndexOf(Toolbar.GetSetting("ToolSize"));
        }

        public override LayerChange[] Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            Coordinates startingCords = coordinates.Length > 1 ? coordinates[1] : coordinates[0];
            var pixels = Draw(startingCords, coordinates[0], color, (int) Toolbar.Settings[_toolSizeIndex].Value);
            return new[] {new LayerChange(pixels, layer)};
        }

        public BitmapPixelChanges Draw(Coordinates startingCoords, Coordinates latestCords, Color color, int toolSize)
        {
            LineTool line = new LineTool();
            return BitmapPixelChanges.FromSingleColoredArray(
                line.CreateLine(new[] { startingCoords, latestCords }, toolSize, 
                    CapType.Square, CapType.Square), color);
        }
    }
}