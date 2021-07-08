using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using PixiEditor.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace PixiEditor.Models.Tools.Tools
{
    public class MagicWandTool : ReadonlyTool
    {
        private readonly FloodFill floodFill;

        public override string ImagePath => $"/Images/Tools/{nameof(FloodFill)}Image.png";

        private static Selection ActiveSelection { get => ViewModelMain.Current.BitmapManager.ActiveDocument.ActiveSelection; }

        private BitmapManager BitmapManager { get; }

        private IEnumerable<Coordinates> oldSelection;

        public SelectionType SelectionType { get; set; } = SelectionType.Add;

        public override void OnRecordingLeftMouseDown(MouseEventArgs e)
        {
            SelectionType = Toolbar.GetEnumSetting<SelectionType>("SelectMode").Value;

            oldSelection = new ReadOnlyCollection<Coordinates>(ActiveSelection.SelectedPoints);
        }

        public override void OnStoppedRecordingMouseUp(MouseEventArgs e)
        {
            if (ActiveSelection.SelectedPoints.Count <= 1)
            {
                // If we have not selected multiple points, clear the selection
                ActiveSelection.Clear();
            }

            SelectionHelpers.AddSelectionUndoStep(ViewModelMain.Current.BitmapManager.ActiveDocument, oldSelection, SelectionType);
        }

        public MagicWandTool(BitmapManager manager)
        {
            floodFill = new FloodFill(manager);
            BitmapManager = manager;

            Toolbar = new SelectToolToolbar(false);
        }

        public override void Use(List<Coordinates> pixels)
        {
            Selection selection = BitmapManager.ActiveDocument.ActiveSelection;

            selection.SetSelection(floodFill.ForestFire(BitmapManager.ActiveLayer, pixels.First(), System.Windows.Media.Colors.White).ChangedPixels.Keys, Toolbar.GetEnumSetting<SelectionType>("SelectMode").Value);
        }
    }
}
