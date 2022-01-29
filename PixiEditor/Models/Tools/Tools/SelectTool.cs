using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using PixiEditor.ViewModels;
using SkiaSharp;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace PixiEditor.Models.Tools.Tools
{
    internal class SelectTool : ReadonlyTool
    {
        private readonly RectangleTool rectangleTool;
        private readonly CircleTool circleTool;
        private IEnumerable<Coordinates> oldSelectedPoints;

        private static Selection ActiveSelection { get => ViewModelMain.Current.BitmapManager.ActiveDocument.ActiveSelection; }

        private BitmapManager BitmapManager { get; }

        public SelectTool(BitmapManager bitmapManager)
        {
            ActionDisplay = "Click and move to select an area.";
            Toolbar = new SelectToolToolbar();
            BitmapManager = bitmapManager;

            rectangleTool = new RectangleTool();
            circleTool = new CircleTool();
        }

        public SelectionType SelectionType { get; set; } = SelectionType.Add;

        public override string Tooltip => "Selects area. (M)";

        public override void BeforeUse()
        {
            base.BeforeUse();
            SelectionType = Toolbar.GetEnumSetting<SelectionType>("SelectMode").Value;

            oldSelectedPoints = new ReadOnlyCollection<Coordinates>(ActiveSelection.SelectedPoints);
        }

        public override void AfterUse(SKRectI sessionRect)
        {
            base.AfterUse(sessionRect);
            if (ActiveSelection.SelectedPoints.Count <= 1)
            {
                // If we have not selected multiple points, clear the selection
                ActiveSelection.Clear();
            }

            SelectionHelpers.AddSelectionUndoStep(ViewModelMain.Current.BitmapManager.ActiveDocument, oldSelectedPoints, SelectionType);
        }

        public override void Use(IReadOnlyList<Coordinates> pixels)
        {
            Select(pixels, Toolbar.GetEnumSetting<SelectionShape>("SelectShape").Value);
        }

        private void Select(IReadOnlyList<Coordinates> pixels, SelectionShape shape)
        {
            Int32Rect rect;
            if (pixels.Count < 2)
            {
                rect = Int32Rect.Empty;
            }
            else
            {
                DoubleCoords fixedCoordinates = ShapeTool.CalculateCoordinatesForShapeRotation(pixels[^1], pixels[0]);
                rect = new(
                    fixedCoordinates.Coords1.X,
                    fixedCoordinates.Coords2.X,
                    fixedCoordinates.Coords2.X - fixedCoordinates.Coords1.X + 1,
                    fixedCoordinates.Coords2.Y - fixedCoordinates.Coords1.Y + 1);
            }

            BitmapManager.ActiveDocument.ActiveSelection.SetSelectionWithUndo(rect, shape, SelectionType);
        }
    }
}
