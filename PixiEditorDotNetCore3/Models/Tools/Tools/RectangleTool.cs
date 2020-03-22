using PixiEditorDotNetCore3.Models.Layers;
using PixiEditorDotNetCore3.Models.Position;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditorDotNetCore3.Models.Tools.Tools
{
    public class RectangleTool : ShapeTool
    {
        public override ToolType ToolType => ToolType.Rectangle;

        public override BitmapPixelChanges Use(Layer layer, Coordinates[] coordinates, Color color, int toolSize)
        {
            CreateRectangle(layer,coordinates[0] ,color,toolSize);
            return new BitmapPixelChanges();
        }

        public void CreateRectangle(Layer layer, Coordinates startingCoords, Color color, int toolSize)
        {
            DoubleCords coordinates = CalculateCoordinatesForShapeRotation(startingCoords);
            layer.LayerBitmap.DrawRectangle(coordinates.Coords1.X, coordinates.Coords1.Y, coordinates.Coords2.X, coordinates.Coords2.Y, color);
        }
    }
}
