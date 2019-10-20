using PixiEditorDotNetCore3.Models.Layers;
using PixiEditorDotNetCore3.Models.Position;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditorDotNetCore3.Models.Tools.Tools
{
    public class CircleTool : ShapeTool
    {
        public override ToolType ToolType => ToolType.Circle;

        public override BitmapPixelChanges Use(Layer layer, Coordinates startingCoords, Color color, int toolSize)
        {
            CreateCircle(layer, startingCoords, color, toolSize);
            return new BitmapPixelChanges();
        }

        public void CreateCircle(Layer layer, Coordinates coordinates, Color color, int size)
        {
            DoubleCords calculatedCords = CalculateCoordinatesForShapeRotation(coordinates);
            layer.LayerBitmap.DrawEllipse(calculatedCords.Coords1.X, calculatedCords.Coords1.Y, calculatedCords.Coords2.X,
                calculatedCords.Coords2.Y, color);
        }

        
    }
}
