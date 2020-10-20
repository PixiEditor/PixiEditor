using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.Models.Colors;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.Tools.Tools
{
    public class BrightnessTool : BitmapOperationTool
    {
        private const float CorrectionFactor = 5f; //Initial correction factor

        private readonly List<Coordinates> pixelsVisited = new List<Coordinates>();

        public BrightnessTool()
        {
            Tooltip = "Makes pixel brighter or darker pixel (U). Hold Ctrl to make pixel darker.";
            Toolbar = new BrightnessToolToolbar(CorrectionFactor);
        }

        public override ToolType ToolType => ToolType.Brightness;
        public BrightnessMode Mode { get; set; } = BrightnessMode.Default;

        public override void OnMouseDown(MouseEventArgs e)
        {
            pixelsVisited.Clear();
        }

        public override LayerChange[] Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            var toolSize = Toolbar.GetSetting<SizeSetting>("ToolSize").Value;
            var correctionFactor = Toolbar.GetSetting<FloatSetting>("CorrectionFactor").Value;
            Enum.TryParse((Toolbar.GetSetting<DropdownSetting>("Mode")?.Value as ComboBoxItem)?.Content as string, out BrightnessMode mode);
            Mode = mode;

            var layersChanges = new LayerChange[1];
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
            var centeredCoords = CoordinatesCalculator.CalculateThicknessCenter(coordinates, toolSize);
            var rectangleCoordinates = CoordinatesCalculator.RectangleToCoordinates(centeredCoords.Coords1.X,
                centeredCoords.Coords1.Y,
                centeredCoords.Coords2.X, centeredCoords.Coords2.Y);
            var changes = new BitmapPixelChanges(new Dictionary<Coordinates, Color>());

            for (var i = 0; i < rectangleCoordinates.Length; i++)
            {
                if (Mode == BrightnessMode.Default)
                {
                    if (pixelsVisited.Contains(rectangleCoordinates[i]))
                        continue;
                    pixelsVisited.Add(rectangleCoordinates[i]);
                }

                var pixel = layer.GetPixelWithOffset(rectangleCoordinates[i].X, rectangleCoordinates[i].Y);
                var newColor = ExColor.ChangeColorBrightness(Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B),
                    correctionFactor);
                changes.ChangedPixels.Add(new Coordinates(rectangleCoordinates[i].X, rectangleCoordinates[i].Y),
                    newColor);
            }

            return changes;
        }
    }
}