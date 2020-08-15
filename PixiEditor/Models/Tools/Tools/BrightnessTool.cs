using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Models.Colors;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.Tools.Tools
{
    public class BrightnessTool : BitmapOperationTool
    {
        private const float CorrectionFactor = 5f; //Initial correction factor

        public override ToolType ToolType => ToolType.Brightness;
        public BrightnessMode Mode { get; set; } = BrightnessMode.Default;

        private List<Coordinates> _pixelsVisited = new List<Coordinates>();

        public BrightnessTool()
        {
            Tooltip = "Makes pixel brighter or darker pixel (U). Hold Ctrl to make pixel darker.";
            Toolbar = new BrightnessToolToolbar(CorrectionFactor);
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            _pixelsVisited.Clear();
        }

        public override LayerChange[] Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            int toolSize = (int) Toolbar.GetSetting("ToolSize").Value;
            float correctionFactor = (float) Toolbar.GetSetting("CorrectionFactor").Value;
            Enum.TryParse<BrightnessMode>((Toolbar.GetSetting("Mode").Value as ComboBoxItem)?.Content as string, out var mode);
            Mode = mode;

            LayerChange[] layersChanges = new LayerChange[1];
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                layersChanges[0] = new LayerChange(ChangeBrightness(layer, coordinates[0], toolSize, -correctionFactor),
                    layer);
            else
                layersChanges[0] = new LayerChange(ChangeBrightness(layer, coordinates[0], toolSize, correctionFactor),
                    layer);
            return layersChanges;
        }

        public BitmapPixelChanges ChangeBrightness(Layer layer, Coordinates coordinates, int toolSize,
            float correctionFactor)
        {
            DoubleCords centeredCoords = CoordinatesCalculator.CalculateThicknessCenter(coordinates, toolSize);
            Coordinates[] rectangleCoordinates = CoordinatesCalculator.RectangleToCoordinates(centeredCoords.Coords1.X,
                centeredCoords.Coords1.Y,
                centeredCoords.Coords2.X, centeredCoords.Coords2.Y);
            BitmapPixelChanges changes = new BitmapPixelChanges(new Dictionary<Coordinates, Color>());

            for (int i = 0; i < rectangleCoordinates.Length; i++)
            {
                if (Mode == BrightnessMode.Default)
                {
                    if(_pixelsVisited.Contains(rectangleCoordinates[i]))
                        continue;
                    _pixelsVisited.Add(rectangleCoordinates[i]);
                }

                Color pixel = layer.GetPixelWithOffset(rectangleCoordinates[i].X, rectangleCoordinates[i].Y);
                Color newColor = ExColor.ChangeColorBrightness(Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B),
                    correctionFactor);
                changes.ChangedPixels.Add(new Coordinates(rectangleCoordinates[i].X, rectangleCoordinates[i].Y),
                    newColor);
            }

            return changes;
        }
    }
}