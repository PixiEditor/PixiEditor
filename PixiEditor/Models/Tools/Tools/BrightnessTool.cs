﻿using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.Helpers.Extensions;
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
        private const float CorrectionFactor = 5f; // Initial correction factor

        private readonly List<Coordinates> pixelsVisited = new List<Coordinates>();

        public BrightnessTool()
        {
            ActionDisplay = "Draw on pixel to make it brighter. Hold Ctrl to darken.";
            Toolbar = new BrightnessToolToolbar(CorrectionFactor);
        }

        public override bool UsesShift => false;
        public override string Tooltip => "Makes pixel brighter or darker pixel (U). Hold Ctrl to make pixel darker.";

        public BrightnessMode Mode { get; set; } = BrightnessMode.Default;

        public override void OnRecordingLeftMouseDown(MouseEventArgs e)
        {
            pixelsVisited.Clear();
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                ActionDisplay = "Draw on pixel to make it darker. Release Ctrl to brighten.";
            }
        }

        public override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                ActionDisplay = "Draw on pixel to make it brighter. Hold Ctrl to darken.";
            }
        }

        public override LayerChange[] Use(Layer layer, List<Coordinates> coordinates, Color color)
        {
            int toolSize = Toolbar.GetSetting<SizeSetting>("ToolSize").Value;
            float correctionFactor = Toolbar.GetSetting<FloatSetting>("CorrectionFactor").Value;
            Mode = Toolbar.GetEnumSetting<BrightnessMode>("BrightnessMode").Value;

            LayerChange[] layersChanges = new LayerChange[1];
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                layersChanges[0] = new LayerChange(ChangeBrightness(layer, coordinates[0], toolSize, -correctionFactor), layer);
            }
            else
            {
                layersChanges[0] = new LayerChange(ChangeBrightness(layer, coordinates[0], toolSize, correctionFactor), layer);
            }

            return layersChanges;
        }

        public BitmapPixelChanges ChangeBrightness(Layer layer, Coordinates coordinates, int toolSize, float correctionFactor)
        {
            DoubleCords centeredCoords = CoordinatesCalculator.CalculateThicknessCenter(coordinates, toolSize);
            IEnumerable<Coordinates> rectangleCoordinates = CoordinatesCalculator.RectangleToCoordinates(
                centeredCoords.Coords1.X,
                centeredCoords.Coords1.Y,
                centeredCoords.Coords2.X,
                centeredCoords.Coords2.Y);
            BitmapPixelChanges changes = new BitmapPixelChanges(new Dictionary<Coordinates, Color>());

            foreach (Coordinates coordinate in rectangleCoordinates)
            {
                if (Mode == BrightnessMode.Default)
                {
                    if (pixelsVisited.Contains(coordinate))
                    {
                        continue;
                    }

                    pixelsVisited.Add(coordinate);
                }

                Color pixel = layer.GetPixelWithOffset(coordinate.X, coordinate.Y);
                Color newColor = ExColor.ChangeColorBrightness(
                    Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B),
                    correctionFactor);
                changes.ChangedPixels.Add(
                    new Coordinates(coordinate.X, coordinate.Y),
                    newColor);
            }

            return changes;
        }
    }
}