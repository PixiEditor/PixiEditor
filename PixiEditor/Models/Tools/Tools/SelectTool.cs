using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.Tools.Tools
{
    public class SelectTool : ReadonlyTool
    {
        public override ToolType ToolType => ToolType.Select;
        private Selection _oldSelection;
        public SelectionType SelectionType = SelectionType.Add;

        public SelectTool()
        {
            Tooltip = "Selects area. (M)";
            Toolbar = new SelectToolToolbar();
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            Enum.TryParse((Toolbar.GetSetting("Mode").Value as ComboBoxItem)?.Content as string, out SelectionType);

            _oldSelection = null;
            if (ViewModelMain.Current.ActiveSelection != null &&
                ViewModelMain.Current.ActiveSelection.SelectedPoints != null)
                _oldSelection = ViewModelMain.Current.ActiveSelection;
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            UndoManager.AddUndoChange(new Change("ActiveSelection", _oldSelection,
                ViewModelMain.Current.ActiveSelection, "Select pixels"));
        }

        public override void Use(Coordinates[] pixels)
        {
            Select(pixels);
        }

        private void Select(Coordinates[] pixels)
        {
            Coordinates[] selection = GetRectangleSelectionForPoints(pixels[^1], pixels[0]);
            ViewModelMain.Current.ActiveSelection.SetSelection(selection, SelectionType);
        }

        public Coordinates[] GetRectangleSelectionForPoints(Coordinates start, Coordinates end)
        {
            RectangleTool rectangleTool = new RectangleTool();
            List<Coordinates> selection = rectangleTool.CreateRectangle(start, end, 1).ToList();
            selection.AddRange(rectangleTool.CalculateFillForRectangle(start, end, 1));
            return selection.ToArray();
        }

        /// <summary>
        ///     Gets coordinates of every pixel in root layer
        /// </summary>
        /// <returns>Coordinates array of pixels</returns>
        public Coordinates[] GetAllSelection()
        {
            return GetAllSelection(ViewModelMain.Current.BitmapManager.ActiveDocument);
        }

        /// <summary>
        ///     Gets coordinates of every pixel in chosen document
        /// </summary>
        /// <param name="document"></param>
        /// <returns>Coordinates array of pixels</returns>
        public Coordinates[] GetAllSelection(Document document)
        {
            return GetRectangleSelectionForPoints(new Coordinates(0, 0), new Coordinates(document.Width - 1, document.Height - 1));
        }
    }
}