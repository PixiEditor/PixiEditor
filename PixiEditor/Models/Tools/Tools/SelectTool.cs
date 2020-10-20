using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.Tools.Tools
{
    public class SelectTool : ReadonlyTool
    {
        private Selection oldSelection;

        public SelectTool()
        {
            Tooltip = "Selects area. (M)";
            Toolbar = new SelectToolToolbar();
        }

        public SelectionType SelectionType { get; set; } = SelectionType.Add;

        public override ToolType ToolType => ToolType.Select;

        public override void OnMouseDown(MouseEventArgs e)
        {
            Enum.TryParse((Toolbar.GetSetting<DropdownSetting>("Mode")?.Value as ComboBoxItem)?.Content as string, out SelectionType selectionType);
            SelectionType = selectionType;

            oldSelection = null;
            if (ViewModelMain.Current.ActiveSelection != null &&
                ViewModelMain.Current.ActiveSelection.SelectedPoints != null)
            {
                oldSelection = ViewModelMain.Current.ActiveSelection;
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (ViewModelMain.Current.ActiveSelection.SelectedPoints.Count() <= 1)
            {
                // If we have not selected multiple points, clear the selection
                ViewModelMain.Current.ActiveSelection.Clear();
            }

            UndoManager.AddUndoChange(new Change("ActiveSelection", oldSelection, ViewModelMain.Current.ActiveSelection, "Select pixels"));
        }

        public override void Use(Coordinates[] pixels)
        {
            Select(pixels);
        }

        public IEnumerable<Coordinates> GetRectangleSelectionForPoints(Coordinates start, Coordinates end)
        {
            RectangleTool rectangleTool = new RectangleTool();
            List<Coordinates> selection = rectangleTool.CreateRectangle(start, end, 1).ToList();
            selection.AddRange(rectangleTool.CalculateFillForRectangle(start, end, 1));
            return selection;
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

        private void Select(Coordinates[] pixels)
        {
            IEnumerable<Coordinates> selection = GetRectangleSelectionForPoints(pixels[^1], pixels[0]);
            ViewModelMain.Current.ActiveSelection.SetSelection(selection, SelectionType);
        }
    }
}