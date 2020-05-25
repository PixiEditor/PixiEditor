using PixiEditor.Models.Colors;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Tools.Tools
{
    public class BrightnessTool : BitmapOperationTool
    {
        public override ToolType ToolType => ToolType.Brightness;
        private const float CorrectionFactor = 5f; //Initial correction factor
        
        public BrightnessTool()
        {
            Tooltip = "Makes pixel brighter or darker pixel (U)";
            Toolbar = new BrightnessToolToolbar(CorrectionFactor);
        }

        public override BitmapPixelChanges Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            int toolSize = (int)Toolbar.GetSetting("ToolSize").Value;
            float correctionFactor = (float)Toolbar.GetSetting("CorrectionFactor").Value;
            if(Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                return ChangeBrightness(layer, coordinates[0], toolSize, -correctionFactor);
            }
                return ChangeBrightness(layer, coordinates[0], toolSize, correctionFactor);
        }       

        private BitmapPixelChanges ChangeBrightness(Layer layer, Coordinates coordinates, int toolSize, float correctionFactor)
        {
            DoubleCords centeredCoords = CoordinatesCalculator.CalculateThicknessCenter(coordinates, toolSize);
            Coordinates[] rectangleCoordinates = CoordinatesCalculator.RectangleToCoordinates(centeredCoords.Coords1.X, centeredCoords.Coords1.Y,
                centeredCoords.Coords2.X, centeredCoords.Coords2.Y);
            BitmapPixelChanges changes = new BitmapPixelChanges(new Dictionary<Coordinates, Color>());

            for (int i = 0; i < rectangleCoordinates.Length; i++)
            {
                Color pixel = layer.LayerBitmap.GetPixel(rectangleCoordinates[i].X, rectangleCoordinates[i].Y);
                Color newColor = ExColor.ChangeColorBrightness(Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B), correctionFactor);
                changes.ChangedPixels.Add(new Coordinates(rectangleCoordinates[i].X, rectangleCoordinates[i].Y), newColor);
            }
            return changes;
        }
    }
}
