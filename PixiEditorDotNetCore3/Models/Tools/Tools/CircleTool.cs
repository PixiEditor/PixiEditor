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
            CreateCircle(coordinates, color, toolSize);
            return new BitmapPixelChanges();
        }

        public void CreateCircle(Coordinates[] coordinates, Color color, int size)
        {
            
        }

        
    }
}
