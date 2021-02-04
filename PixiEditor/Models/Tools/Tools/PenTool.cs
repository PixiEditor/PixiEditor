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
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.Tools.Tools
{
    public class PenTool : BitmapOperationTool
    {
        private readonly SizeSetting toolSizeSetting;
        private readonly BoolSetting pixelPerfectSetting;
        private readonly LineTool lineTool;
        private IEnumerable<Coordinates> lastCords = Array.Empty<Coordinates>();

        public PenTool()
        {
            Cursor = Cursors.Pen;
            ActionDisplay = "Click and move to draw.";
            Tooltip = "Standard brush. (B)";
            Toolbar = new PenToolbar();
            toolSizeSetting = Toolbar.GetSetting<SizeSetting>("ToolSize");
            pixelPerfectSetting = Toolbar.GetSetting<BoolSetting>("PixelPerfectEnabled");
            lineTool = new LineTool();
            RequiresPreviewLayer = true;
            ClearPreviewLayerOnEachIteration = false;
        }

        public override void AfterAddedUndo(UndoManager undoManager)
        {
            base.AfterAddedUndo(undoManager);
            lastCords = Array.Empty<Coordinates>();
        }

        public override LayerChange[] Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            Coordinates startingCords = coordinates.Length > 1 ? coordinates[1] : coordinates[0];
            BitmapPixelChanges pixels = Draw(startingCords, coordinates[0], color, toolSizeSetting.Value);
            return Only(pixels, layer);
        }

        public BitmapPixelChanges Draw(Coordinates startingCoords, Coordinates latestCords, Color color, int toolSize)
        {
            return BitmapPixelChanges.FromSingleColoredArray(
                lineTool.CreateLine(startingCoords, latestCords, toolSize), color);
        }
    }
}