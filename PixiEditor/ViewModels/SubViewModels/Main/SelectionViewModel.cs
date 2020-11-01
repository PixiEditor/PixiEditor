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

        private Selection selection;

        public Selection ActiveSelection
        {
            get => selection;
            set
            {
                selection = value;
                RaisePropertyChanged("ActiveSelection");
            }
        }

        public SelectionViewModel(ViewModelMain owner)
            : base(owner)
        {
            DeselectCommand = new RelayCommand(Deselect, SelectionIsNotEmpty);
            SelectAllCommand = new RelayCommand(SelectAll, CanSelectAll);
            ActiveSelection = new Selection(Array.Empty<Coordinates>());
        }

        public void SelectAll(object parameter)
        {
            SelectTool select = new SelectTool();
            ActiveSelection.SetSelection(select.GetAllSelection(), SelectionType.New);
        }

        public void Deselect(object parameter)
        {
            ActiveSelection?.Clear();
        }

        public bool SelectionIsNotEmpty(object property)
        {
            return ActiveSelection?.SelectedPoints != null && ActiveSelection.SelectedPoints.Count > 0;
        }

        private bool CanSelectAll(object property)
        {
            return Owner.BitmapManager.ActiveDocument != null && Owner.BitmapManager.ActiveDocument.Layers.Count > 0;
        }
    }
}