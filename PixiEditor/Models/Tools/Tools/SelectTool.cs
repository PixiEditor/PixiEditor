using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using PixiEditor.Models.Undo;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.Tools.Tools
{
    public class SelectTool : ReadonlyTool
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

        public override void OnRecordingLeftMouseDown(MouseEventArgs e)
        {
            SelectionType = Toolbar.GetEnumSetting<SelectionType>("SelectMode").Value;

            oldSelectedPoints = new ReadOnlyCollection<Coordinates>(ActiveSelection.SelectedPoints);
        }

        public override void OnStoppedRecordingMouseUp(MouseEventArgs e)
        {
            if (ActiveSelection.SelectedPoints.Count <= 1)
            {
                // If we have not selected multiple points, clear the selection
                ActiveSelection.Clear();
            }

            SelectionHelpers.AddSelectionUndoStep(ViewModelMain.Current.BitmapManager.ActiveDocument, oldSelectedPoints, SelectionType);
        }

        public override void Use(List<Coordinates> pixels)
        {
            Select(pixels, Toolbar.GetEnumSetting<SelectionShape>("SelectShape").Value);
        }

        public IEnumerable<Coordinates> GetRectangleSelectionForPoints(Coordinates start, Coordinates end)
        {
            List<Coordinates> result = new List<Coordinates>();
            ToolCalculator.GenerateRectangleNonAlloc(
                start, end, true, 1, result);
            return result;
        }

        public IEnumerable<Coordinates> GetCircleSelectionForPoints(Coordinates start, Coordinates end)
        {
            List<Coordinates> result = new List<Coordinates>();
            ToolCalculator.GenerateEllipseNonAlloc(
                start, end, true, result);
            return result;
        }

        /// <summary>
        ///     Gets coordinates of every pixel in root layer.
        /// </summary>
        /// <returns>Coordinates array of pixels.</returns>
        public IEnumerable<Coordinates> GetAllSelection()
        {
            return GetAllSelection(ViewModelMain.Current.BitmapManager.ActiveDocument);
        }

        /// <summary>
        ///     Gets coordinates of every pixel in chosen document.
        /// </summary>
        /// <returns>Coordinates array of pixels.</returns>
        public IEnumerable<Coordinates> GetAllSelection(Document document)
        {
            return GetRectangleSelectionForPoints(new Coordinates(0, 0), new Coordinates(document.Width - 1, document.Height - 1));
        }

        private void Select(List<Coordinates> pixels, SelectionShape shape)
        {
            IEnumerable<Coordinates> selection;

            BitmapManager.ActiveDocument.ActiveSelection.SetSelection(oldSelectedPoints, SelectionType.New);

            if (pixels.Count < 2)
            {
                selection = new List<Coordinates>();
            }
            else if (shape == SelectionShape.Circle)
            {
                selection = GetCircleSelectionForPoints(pixels[^1], pixels[0]);
            }
            else if (shape == SelectionShape.Rectangle)
            {
                selection = GetRectangleSelectionForPoints(pixels[^1], pixels[0]);
            }
            else
            {
                throw new NotImplementedException($"Selection shape '{shape}' has not been implemented");
            }

            BitmapManager.ActiveDocument.ActiveSelection.SetSelection(selection, SelectionType);
        }
    }
}