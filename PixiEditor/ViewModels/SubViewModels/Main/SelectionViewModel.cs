using System;
using System.Collections.Generic;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class SelectionViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand DeselectCommand { get; set; }

        public RelayCommand SelectAllCommand { get; set; }

        private readonly SelectTool selectTool;

        public SelectionViewModel(ViewModelMain owner)
            : base(owner)
        {
            DeselectCommand = new RelayCommand(Deselect, SelectionIsNotEmpty);
            SelectAllCommand = new RelayCommand(SelectAll, CanSelectAll);

            selectTool = new SelectTool(Owner.BitmapManager);
        }

        public void SelectAll(object parameter)
        {
            var oldSelection = new List<Coordinates>(Owner.BitmapManager.ActiveDocument.ActiveSelection.SelectedPoints);

            Owner.BitmapManager.ActiveDocument.ActiveSelection.SetSelection(selectTool.GetAllSelection(), SelectionType.New);
            SelectionHelpers.AddSelectionUndoStep(Owner.BitmapManager.ActiveDocument, oldSelection, SelectionType.New);
        }

        public void Deselect(object parameter)
        {
            var oldSelection = new List<Coordinates>(Owner.BitmapManager.ActiveDocument.ActiveSelection.SelectedPoints);

            Owner.BitmapManager.ActiveDocument.ActiveSelection?.Clear();

            SelectionHelpers.AddSelectionUndoStep(Owner.BitmapManager.ActiveDocument, oldSelection, SelectionType.New);
        }

        public bool SelectionIsNotEmpty(object property)
        {
            var selectedPoints = Owner.BitmapManager.ActiveDocument?.ActiveSelection.SelectedPoints;
            return selectedPoints != null && selectedPoints.Count > 0;
        }

        private bool CanSelectAll(object property)
        {
            return Owner.BitmapManager.ActiveDocument != null && Owner.BitmapManager.ActiveDocument.Layers.Count > 0;
        }
    }
}