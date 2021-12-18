using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Colors;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PixiEditor.Models.Tools.Tools
{
    public class BrightnessTool : BitmapOperationTool
    {
        private const float CorrectionFactor = 5f; // Initial correction factor

        private readonly string defaultActionDisplay = "Draw on pixels to make them brighter. Hold Ctrl to darken.";
        private readonly List<Coordinates> pixelsVisited = new List<Coordinates>();
        private List<DoubleCoords> circleCache = new List<DoubleCoords>();
        private int cachedCircleSize = -1;

        public BrightnessTool()
        {
            ActionDisplay = defaultActionDisplay;
            Toolbar = new BrightnessToolToolbar(CorrectionFactor);
        }

        public override string Tooltip => "Makes pixels brighter or darker (U). Hold Ctrl to make pixels darker.";

        public BrightnessMode Mode { get; set; } = BrightnessMode.Default;

        public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
        {
            if (!ctrlIsDown)
                ActionDisplay = defaultActionDisplay;
            else
                ActionDisplay = "Draw on pixels to make them darker. Release Ctrl to brighten.";
        }

        public override void BeforeUse()
        {
            base.BeforeUse();
            pixelsVisited.Clear();
        }

        public override void Use(Layer activeLayer, Layer previewLayer, IEnumerable<Layer> allLayers, IReadOnlyList<Coordinates> recordedMouseMovement, SKColor color)
        {
            int toolSize = Toolbar.GetSetting<SizeSetting>("ToolSize").Value;
            float correctionFactor = Toolbar.GetSetting<FloatSetting>("CorrectionFactor").Value;
            Mode = Toolbar.GetEnumSetting<BrightnessMode>("BrightnessMode").Value;

            if (Session.IsCtrlDown)
            {
                ChangeBrightness(activeLayer, recordedMouseMovement[^1], toolSize, -correctionFactor);
            }
            else
            {
                ChangeBrightness(activeLayer, recordedMouseMovement[^1], toolSize, correctionFactor);
            }
        }

        private void ChangeBrightness(Layer layer, Coordinates coordinates, int toolSize, float correctionFactor)
        {
            if (cachedCircleSize != toolSize)
                UpdateCircleCache(toolSize);

            int radius = (int)Math.Ceiling(toolSize / 2f);
            Int32Rect dirtyRect = new(coordinates.X - radius, coordinates.Y - radius, radius * 2, radius * 2);
            layer.DynamicResizeAbsolute(dirtyRect);

            foreach (var pair in circleCache)
            {
                Coordinates left = pair.Coords1;
                Coordinates right = pair.Coords2;
                int y = left.Y + coordinates.Y;

                for (int x = left.X + coordinates.X; x <= right.X + coordinates.X; x++)
                {
                    if (Mode == BrightnessMode.Default)
                    {
                        Coordinates here = new(x, y);
                        if (pixelsVisited.Contains(here))
                            continue;

                        pixelsVisited.Add(here);
                    }

                    SKColor pixel = layer.GetPixelWithOffset(x, y);
                    SKColor newColor = ExColor.ChangeColorBrightness(
                        pixel,
                        correctionFactor);
                    layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(x - layer.OffsetX, y - layer.OffsetY, newColor);
                }
            }
            layer.InvokeLayerBitmapChange(dirtyRect);
        }

        private void UpdateCircleCache(int newCircleSize)
        {
            cachedCircleSize = newCircleSize;
            DoubleCoords rect = CoordinatesCalculator.CalculateThicknessCenter(new Coordinates(0, 0), newCircleSize);
            List<Coordinates> circle = EllipseGenerator.GenerateEllipseFromRect(rect);
            circleCache = EllipseGenerator.SplitEllipseIntoLines(circle);
        }
    }
}
