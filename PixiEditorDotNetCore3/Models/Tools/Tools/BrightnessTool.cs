using PixiEditorDotNetCore3.Models.Colors;
using PixiEditorDotNetCore3.Models.Layers;
using PixiEditorDotNetCore3.Models.Position;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditorDotNetCore3.Models.Tools.Tools
{
    public class BrightnessTool : Tool
    {
        public override ToolType ToolType => ToolType.Brightness;
        public const float DarkenFactor = -0.06f;
        public const float LightenFactor = 0.1f;

        public override BitmapPixelChanges Use(Layer layer, Coordinates startingCoords, Color color, int toolSize)
        {
            if(Mouse.LeftButton == MouseButtonState.Pressed)
            {
                return ChangeBrightness(layer, startingCoords, toolSize, LightenFactor);
            }
                return ChangeBrightness(layer, startingCoords, toolSize, DarkenFactor);
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
