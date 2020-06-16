using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings;

namespace PixiEditor.Models.Tools.Tools
{
    public class PenTool : BitmapOperationTool
    {
        private readonly int _toolSizeIndex;

        public PenTool()
        {
            Cursor = Cursors.Pen;
            Tooltip = "Standard brush (B)";
            Toolbar = new BasicToolbar();
            _toolSizeIndex = Toolbar.Settings.IndexOf(Toolbar.GetSetting("ToolSize"));
        }

        public override ToolType ToolType => ToolType.Pen;

        public override LayerChange[] Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            var pixels = Draw(coordinates[0], color, (int) Toolbar.Settings[_toolSizeIndex].Value);
            return new[] {new LayerChange(pixels, layer)};
        }

        public BitmapPixelChanges Draw(Coordinates startingCoords, Color color, int toolSize)
        {
            int x1, y1, x2, y2;
            DoubleCords centeredCoords = CoordinatesCalculator.CalculateThicknessCenter(startingCoords, toolSize);
            x1 = centeredCoords.Coords1.X;
            y1 = centeredCoords.Coords1.Y;
            x2 = centeredCoords.Coords2.X;
            y2 = centeredCoords.Coords2.Y;
            return BitmapPixelChanges.FromSingleColoredArray(
                CoordinatesCalculator.RectangleToCoordinates(x1, y1, x2, y2), color);
        }
    }
}