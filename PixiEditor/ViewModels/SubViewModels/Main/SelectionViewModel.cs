using System;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.Tools;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class SelectionViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand DeselectCommand { get; set; }

        public RelayCommand SelectAllCommand { get; set; }

        public SelectionViewModel(ViewModelMain owner)
            : base(owner)
        {
            DeselectCommand = new RelayCommand(Deselect, SelectionIsNotEmpty);
            SelectAllCommand = new RelayCommand(SelectAll, CanSelectAll);
        }

        public void SelectAll(object parameter)
        {
            SelectTool select = new SelectTool();
            Owner.BitmapManager.ActiveDocument.ActiveSelection.SetSelection(select.GetAllSelection(), SelectionType.New);
        }

        public void Deselect(object parameter)
        {
            Owner.BitmapManager.ActiveDocument.ActiveSelection?.Clear();
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