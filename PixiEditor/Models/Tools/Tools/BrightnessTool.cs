using PixiEditor.Models.Colors;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Tools.Tools
{
    public class BrightnessTool : Tool
    {
        public override ToolType ToolType => ToolType.Brightness;
        public const float DarkenFactor = -0.06f;
        public const float LightenFactor = 0.1f;
        
        public BrightnessTool()
        {
            Tooltip = "Makes pixel brighter or darker pixel (U)";
        }

        public override BitmapPixelChanges Use(Layer layer, Coordinates[] coordinates, Color color, int toolSize)
        {
            if(Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                return ChangeBrightness(layer, coordinates[0], toolSize, DarkenFactor);
            }
                return ChangeBrightness(layer, coordinates[0], toolSize, LightenFactor);
        }       

        private BitmapPixelChanges ChangeBrightness(Layer layer, Coordinates coordinates, int toolSize, float correctionFactor)
        {
            PenTool pen = new PenTool();
            Color pixel = layer.LayerBitmap.GetPixel(coordinates.X, coordinates.Y);
            Color newColor = ExColor.ChangeColorBrightness(Color.FromArgb(pixel.A,pixel.R, pixel.G, pixel.B), correctionFactor);
            return pen.Draw(coordinates, newColor, toolSize);
        }
    }
}
