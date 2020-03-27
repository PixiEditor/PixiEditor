using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Tools.Tools
{
    public class LineTool : ShapeTool
    {
        public override ToolType ToolType => ToolType.Line;

        public override BitmapPixelChanges Use(Layer layer, Coordinates[] coordinates, Color color, int toolSize)
        {
            return BitmapPixelChanges.FromSingleColoredArray(CreateLine(coordinates), color);
        }

        public Coordinates[] CreateLine(Coordinates[] coordinates)
        {
            Coordinates startingCoordinates = coordinates[^1];
            Coordinates latestCoordinates = coordinates[0];
            return BresenhamLine(startingCoordinates.X, startingCoordinates.Y, latestCoordinates.X, latestCoordinates.Y);
        }
    }
}
