using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace PixiEditor.Models.Tools.Tools
{
    public class PenTool : ShapeTool
    {
        private readonly SizeSetting toolSizeSetting;
        private readonly BoolSetting pixelPerfectSetting;
        private readonly List<Coordinates> confirmedPixels = new List<Coordinates>();
        private readonly LineTool lineTool;
        private Coordinates[] lastChangedPixels = new Coordinates[3];
        private byte changedPixelsindex;

        private BitmapManager BitmapManager { get; }

        public PenTool(BitmapManager bitmapManager)
        {
            Cursor = Cursors.Pen;
            ActionDisplay = "Click and move to draw.";
            Toolbar = new PenToolbar();
            toolSizeSetting = Toolbar.GetSetting<SizeSetting>("ToolSize");
            pixelPerfectSetting = Toolbar.GetSetting<BoolSetting>("PixelPerfectEnabled");
            pixelPerfectSetting.ValueChanged += PixelPerfectSettingValueChanged;
            ClearPreviewLayerOnEachIteration = false;
            BitmapManager = bitmapManager;
            lineTool = new LineTool();
        }

        public override string Tooltip => "Standard brush. (B)";
        public override bool UsesShift => false;


        public override void OnRecordingLeftMouseDown(MouseEventArgs e)
        {
            base.OnRecordingLeftMouseDown(e);
            changedPixelsindex = 0;
            lastChangedPixels = new Coordinates[3];
            confirmedPixels.Clear();
        }

        public override void Use(Layer layer, List<Coordinates> coordinates, Color color)
        {
            Coordinates startingCords = coordinates.Count > 1 ? coordinates[1] : coordinates[0];
            Draw(
                layer,
                startingCords,
                coordinates[0],
                color,
                toolSizeSetting.Value,
                pixelPerfectSetting.Value,
                BitmapManager.ActiveDocument.PreviewLayer);
        }

        public void Draw(Layer layer, Coordinates startingCoords, Coordinates latestCords, Color color, int toolSize, bool pixelPerfect = false, Layer previewLayer = null)
        {
            if (!pixelPerfect)
            {
                lineTool.CreateLine(layer, color, startingCoords, latestCords, toolSize);
                return;
            }

            if (previewLayer != null && previewLayer.GetPixelWithOffset(latestCords.X, latestCords.Y).A > 0)
            {
                confirmedPixels.Add(latestCords);
            }

            var latestPixels = lineTool.CreateLine(layer, color, startingCoords, latestCords, 1);
            SetPixelToCheck(latestPixels);

            if (changedPixelsindex == 2)
            {
                var changes = ApplyPixelPerfectToPixels(
                    layer,
                    lastChangedPixels[0],
                    lastChangedPixels[1],
                    lastChangedPixels[2],
                    color,
                    toolSize);

                MovePixelsToCheck(changes);

                ThickenShape(layer, color, latestPixels, toolSize);
                return;
            }

            changedPixelsindex += changedPixelsindex >= 2 ? (byte)0 : (byte)1;

            ThickenShape(layer, color, latestPixels, toolSize);
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

        private BitmapPixelChanges ApplyPixelPerfectToPixels(Layer layer, Coordinates p1, Coordinates p2, Coordinates p3, Color color, int toolSize)
        {
            if (Math.Abs(p3.X - p1.X) == 1 && Math.Abs(p3.Y - p1.Y) == 1 && !confirmedPixels.Contains(p2))
            {
                ThickenShape(layer, color, new Coordinates[] { p1, p3 }, toolSize);
                ThickenShape(layer, color, new[] { p2 }, toolSize);
            }

            ThickenShape(layer, color, new Coordinates[] { p2, p3 }.Distinct(), toolSize);
        }

        private void PixelPerfectSettingValueChanged(object sender, SettingValueChangedEventArgs<bool> e)
        {
            RequiresPreviewLayer = e.NewValue;
        }
    }
}
