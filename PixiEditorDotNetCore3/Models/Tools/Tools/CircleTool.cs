using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Tools.Tools
{
    public class CircleTool : ShapeTool
    {
        public override ToolType ToolType => ToolType.Circle;

        public override BitmapPixelChanges Use(Layer layer, Coordinates[] coordinates, Color color, int toolSize)
        {

            return BitmapPixelChanges.FromSingleColoredArray(CreateCircle(coordinates, toolSize), color);
        }

        public Coordinates[] CreateCircle(Coordinates[] coordinates, int size)
        {
            return System.Array.Empty<Coordinates>();
        }

        
    }
}
