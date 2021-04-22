using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.Tools.Tools
{
    public class PenTool : ShapeTool
    {
        private readonly SizeSetting toolSizeSetting;
        private readonly BoolSetting pixelPerfectSetting;
        private readonly LineTool lineTool;
        private Coordinates[] lastChangedPixels = new Coordinates[3];
        private readonly List<Coordinates> confirmedPixels = new List<Coordinates>();
        private byte changedPixelsindex = 0;

        public PenTool()
        {
            Cursor = Cursors.Pen;
            ActionDisplay = "Click and move to draw.";
            Tooltip = "Standard brush. (B)";
            Toolbar = new PenToolbar();
            toolSizeSetting = Toolbar.GetSetting<SizeSetting>("ToolSize");
            pixelPerfectSetting = Toolbar.GetSetting<BoolSetting>("PixelPerfectEnabled");
            pixelPerfectSetting.ValueChanged += PixelPerfectSettingValueChanged;
            lineTool = new LineTool();
            ClearPreviewLayerOnEachIteration = false;
        }

        public override void OnRecordingLeftMouseDown(MouseEventArgs e)
        {
            base.OnRecordingLeftMouseDown(e);
            changedPixelsindex = 0;
            lastChangedPixels = new Coordinates[3];
            confirmedPixels.Clear();
        }

        public override LayerChange[] Use(Layer layer, List<Coordinates> coordinates, Color color)
        {
            Coordinates startingCords = coordinates.Count > 1 ? coordinates[1] : coordinates[0];
            BitmapPixelChanges pixels = Draw(
                startingCords,
                coordinates[0],
                color,
                toolSizeSetting.Value,
                pixelPerfectSetting.Value,
                ViewModelMain.Current.BitmapManager.ActiveDocument.PreviewLayer);
            return Only(pixels, layer);
        }

        public BitmapPixelChanges Draw(Coordinates startingCoords, Coordinates latestCords, Color color, int toolSize, bool pixelPerfect = false, Layer previewLayer = null)
        {
            if (!pixelPerfect)
            {
                return BitmapPixelChanges.FromSingleColoredArray(
                    lineTool.CreateLine(startingCoords, latestCords, toolSize), color);
            }

            if (previewLayer != null && previewLayer.GetPixelWithOffset(latestCords.X, latestCords.Y).A > 0)
            {
                confirmedPixels.Add(latestCords);
            }

            var latestPixels = lineTool.CreateLine(startingCoords, latestCords, 1);
            SetPixelToCheck(latestPixels);

            if (changedPixelsindex == 2)
            {
                var changes = ApplyPixelPerfectToPixels(
                    lastChangedPixels[0],
                    lastChangedPixels[1],
                    lastChangedPixels[2],
                    color,
                    toolSize);

                MovePixelsToCheck(changes);

                changes.ChangedPixels.AddRangeNewOnly(
                    BitmapPixelChanges.FromSingleColoredArray(GetThickShape(latestPixels, toolSize), color).ChangedPixels);

                return changes;
            }

            changedPixelsindex += changedPixelsindex >= 2 ? (byte)0 : (byte)1;

            var result = BitmapPixelChanges.FromSingleColoredArray(GetThickShape(latestPixels, toolSize), color);

            return result;
        }

        private void MovePixelsToCheck(BitmapPixelChanges changes)
        {
            if (changes.ChangedPixels[lastChangedPixels[1]].A != 0)
            {
                lastChangedPixels[0] = lastChangedPixels[1];
                lastChangedPixels[1] = lastChangedPixels[2];
                changedPixelsindex = 2;
            }
            else
            {
                lastChangedPixels[0] = lastChangedPixels[2];
                changedPixelsindex = 1;
            }
        }

        private void SetPixelToCheck(IEnumerable<Coordinates> latestPixels)
        {
            if (latestPixels.Count() == 1)
            {
                lastChangedPixels[changedPixelsindex] = latestPixels.First();
            }
            else
            {
                lastChangedPixels[changedPixelsindex] = latestPixels.ElementAt(1);
            }
        }

        private BitmapPixelChanges ApplyPixelPerfectToPixels(Coordinates p1, Coordinates p2, Coordinates p3, Color color, int toolSize)
        {
            if (Math.Abs(p3.X - p1.X) == 1 && Math.Abs(p3.Y - p1.Y) == 1 && !confirmedPixels.Contains(p2))
            {
                var changes = BitmapPixelChanges.FromSingleColoredArray(GetThickShape(new Coordinates[] { p1, p3 }, toolSize), color);
                changes.ChangedPixels.AddRangeNewOnly(
                    BitmapPixelChanges.FromSingleColoredArray(
                        GetThickShape(new[] { p2 }, toolSize),
                        System.Windows.Media.Colors.Transparent).ChangedPixels);
                return changes;
            }

            return BitmapPixelChanges.FromSingleColoredArray(GetThickShape(new Coordinates[] { p2, p3 }.Distinct(), toolSize), color);
        }

        private void PixelPerfectSettingValueChanged(object sender, SettingValueChangedEventArgs<bool> e)
        {
            RequiresPreviewLayer = e.NewValue;
        }
    }
}