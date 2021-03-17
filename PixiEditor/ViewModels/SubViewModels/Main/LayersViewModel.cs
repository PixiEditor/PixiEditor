using System;
using System.Linq;
using System.Windows.Input;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Layers;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class LayersViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand SetActiveLayerCommand { get; set; }

        public RelayCommand NewLayerCommand { get; set; }

        public RelayCommand NewFolderCommand { get; set; }

        public RelayCommand DeleteLayersCommand { get; set; }

        public RelayCommand RenameLayerCommand { get; set; }

        public RelayCommand MoveToBackCommand { get; set; }

        public RelayCommand MoveToFrontCommand { get; set; }

        public RelayCommand MergeSelectedCommand { get; set; }

        public RelayCommand MergeWithAboveCommand { get; set; }

        public RelayCommand MergeWithBelowCommand { get; set; }

        public LayersViewModel(ViewModelMain owner)
            : base(owner)
        {
            SetActiveLayerCommand = new RelayCommand(SetActiveLayer);
            NewLayerCommand = new RelayCommand(NewLayer, CanCreateNewLayer);
            NewFolderCommand = new RelayCommand(NewFolder, CanCreateNewLayer);
            DeleteLayersCommand = new RelayCommand(DeleteLayer, CanDeleteLayer);
            MoveToBackCommand = new RelayCommand(MoveLayerToBack, CanMoveToBack);
            MoveToFrontCommand = new RelayCommand(MoveLayerToFront, CanMoveToFront);
            RenameLayerCommand = new RelayCommand(RenameLayer);
            MergeSelectedCommand = new RelayCommand(MergeSelected, CanMergeSelected);
            MergeWithAboveCommand = new RelayCommand(MergeWithAbove, CanMergeWithAbove);
            MergeWithBelowCommand = new RelayCommand(MergeWithBelow, CanMergeWithBelow);
            Owner.BitmapManager.DocumentChanged += BitmapManager_DocumentChanged;
        }

        public void NewFolder(object parameter)
        {
            Owner.BitmapManager.ActiveDocument?.LayerStructure.Folders.Add(new GuidStructureItem("New Folder"));
        }

        public bool CanMergeSelected(object obj)
        {
            return Owner.BitmapManager.ActiveDocument?.Layers.Count(x => x.IsActive) > 1;
        }

        public void NewLayer(object parameter)
        {
            Owner.BitmapManager.ActiveDocument.AddNewLayer($"New Layer {Owner.BitmapManager.ActiveDocument.Layers.Count}");
        }

        public bool CanCreateNewLayer(object parameter)
        {
            return Owner.BitmapManager.ActiveDocument != null;
        }

        public void SetActiveLayer(object parameter)
        {
            int index = (int)parameter;

            if (Owner.BitmapManager.ActiveDocument.Layers[index].IsActive && Mouse.RightButton == MouseButtonState.Pressed)
            {
                return;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Owner.BitmapManager.ActiveDocument.ToggleLayer(index);
            }
            else if (Keyboard.IsKeyDown(Key.LeftShift) && Owner.BitmapManager.ActiveDocument.Layers.Any(x => x.IsActive))
            {
                Owner.BitmapManager.ActiveDocument.SelectLayersRange(index);
            }
            else
            {
                Owner.BitmapManager.ActiveDocument.SetMainActiveLayer(index);
            }
        }

        public void DeleteLayer(object parameter)
        {
            int index = (int)parameter;
            if (!Owner.BitmapManager.ActiveDocument.Layers[index].IsActive)
            {
                Owner.BitmapManager.ActiveDocument.RemoveLayer(index);
            }
            else
            {
                Owner.BitmapManager.ActiveDocument.RemoveActiveLayers();
            }
        }

        public bool CanDeleteLayer(object property)
        {
            return Owner.BitmapManager.ActiveDocument != null && Owner.BitmapManager.ActiveDocument.Layers.Count > 1;
        }

        public void RenameLayer(object parameter)
        {
            int? index = (int?)parameter;

            if (index == null)
            {
                index = Owner.BitmapManager.ActiveDocument.Layers.IndexOf(Owner.BitmapManager.ActiveDocument.ActiveLayer);
            }

            Owner.BitmapManager.ActiveDocument.Layers[(int)index].IsRenaming = true;
        }

        public bool CanRenameLayer(object parameter)
        {
            return Owner.BitmapManager.ActiveDocument != null;
        }

        public void MoveLayerToFront(object parameter)
        {
            int oldIndex = (int)parameter;
            Owner.BitmapManager.ActiveDocument.MoveLayerIndexBy(oldIndex, 1);
        }

        public void MoveLayerToBack(object parameter)
        {
            int oldIndex = (int)parameter;
            Owner.BitmapManager.ActiveDocument.MoveLayerIndexBy(oldIndex, -1);
        }

        public bool CanMoveToFront(object property)
        {
            if (property == null)
            {
                return false;
            }

            return Owner.DocumentIsNotNull(null) && Owner.BitmapManager.ActiveDocument.Layers.Count - 1 > (int)property;
        }

        public bool CanMoveToBack(object property)
        {
            if (property == null)
            {
                return false;
            }

            return (int)property > 0;
        }

        public void MergeSelected(object parameter)
        {
            Owner.BitmapManager.ActiveDocument.MergeLayers(Owner.BitmapManager.ActiveDocument.Layers.Where(x => x.IsActive).ToArray(), false);
        }

        public void MergeWithAbove(object parameter)
        {
            int index = (int)parameter;
            Layer layer1 = Owner.BitmapManager.ActiveDocument.Layers[index];
            Layer layer2 = Owner.BitmapManager.ActiveDocument.Layers[index + 1];
            Owner.BitmapManager.ActiveDocument.MergeLayers(new Layer[] { layer1, layer2 }, false);
        }

        public void MergeWithBelow(object parameter)
        {
            int index = (int)parameter;
            Layer layer1 = Owner.BitmapManager.ActiveDocument.Layers[index - 1];
            Layer layer2 = Owner.BitmapManager.ActiveDocument.Layers[index];
            Owner.BitmapManager.ActiveDocument.MergeLayers(new Layer[] { layer1, layer2 }, true);
        }

        public bool CanMergeWithAbove(object property)
        {
            if (property == null)
            {
                return false;
            }
            int index = (int)property;
            return Owner.DocumentIsNotNull(null) && index != Owner.BitmapManager.ActiveDocument.Layers.Count - 1
                && Owner.BitmapManager.ActiveDocument.Layers.Count(x => x.IsActive) == 1;
        }

        public bool CanMergeWithBelow(object property)
        {
            if (property == null)
            {
                return false;
            }

            int index = (int)property;
            return Owner.DocumentIsNotNull(null) && index != 0 && Owner.BitmapManager.ActiveDocument.Layers.Count(x => x.IsActive) == 1;
        }

        private void BitmapManager_DocumentChanged(object sender, Models.Events.DocumentChangedEventArgs e)
        {
            if (e.OldDocument != null)
            {
                e.OldDocument.LayersChanged -= Document_LayersChanged;
            }

            if (e.NewDocument != null)
            {
                e.NewDocument.LayersChanged += Document_LayersChanged;
            }
        }

        private void Document_LayersChanged(object sender, LayersChangedEventArgs e)
        {
            if (e.LayerChangeType == Models.Enums.LayerAction.SetActive)
            {
                Owner.BitmapManager.ActiveDocument.UpdateLayersColor();
            }
        }
    }
}