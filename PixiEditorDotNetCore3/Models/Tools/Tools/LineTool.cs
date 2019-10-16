using PixiEditorDotNetCore3.Models.Layers;
using PixiEditorDotNetCore3.Models.Position;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditorDotNetCore3.Models.Tools.Tools
{
    public class LineTool : ShapeTool
    {
        public override ToolType ToolType => ToolType.Line;

        public override BitmapPixelChanges Use(Layer layer, Coordinates startingCoords, Color color, int toolSize)
        {
            CreateLine(layer, startingCoords, color, toolSize);
            return new BitmapPixelChanges();
        }

        public void CreateLine(Layer layer, Coordinates coordinates, Color color, int size)
        {           
            layer.LayerBitmap.DrawLineBresenham(coordinates.X, coordinates.Y, MousePositionConverter.CurrentCoordinates.X,
                MousePositionConverter.CurrentCoordinates.Y, color);
        }
    }
}
