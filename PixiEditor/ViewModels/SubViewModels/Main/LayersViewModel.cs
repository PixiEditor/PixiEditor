using PixiEditor.Helpers;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class LayersViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand SetActiveLayerCommand { get; set; }

        public RelayCommand NewLayerCommand { get; set; }

        public RelayCommand DeleteLayerCommand { get; set; }

        public RelayCommand RenameLayerCommand { get; set; }

        public RelayCommand MoveToBackCommand { get; set; }

        public RelayCommand MoveToFrontCommand { get; set; }

        public LayersViewModel(ViewModelMain owner)
            : base(owner)
        {
            SetActiveLayerCommand = new RelayCommand(SetActiveLayer);
            NewLayerCommand = new RelayCommand(NewLayer, CanCreateNewLayer);
            DeleteLayerCommand = new RelayCommand(DeleteLayer, CanDeleteLayer);
            MoveToBackCommand = new RelayCommand(MoveLayerToBack, CanMoveToBack);
            MoveToFrontCommand = new RelayCommand(MoveLayerToFront, CanMoveToFront);
            RenameLayerCommand = new RelayCommand(RenameLayer);
        }

        public void NewLayer(object parameter)
        {
            Owner.BitmapManager.AddNewLayer($"New Layer {Owner.BitmapManager.ActiveDocument.Layers.Count}");
        }

        public bool CanCreateNewLayer(object parameter)
        {
            return Owner.BitmapManager.ActiveDocument != null && Owner.BitmapManager.ActiveDocument.Layers.Count > 0;
        }

        public void SetActiveLayer(object parameter)
        {
            Owner.BitmapManager.SetActiveLayer((int)parameter);
        }

        public void DeleteLayer(object parameter)
        {
            Owner.BitmapManager.RemoveLayer((int)parameter);
        }

        public bool CanDeleteLayer(object property)
        {
            return Owner.BitmapManager.ActiveDocument != null && Owner.BitmapManager.ActiveDocument.Layers.Count > 1;
        }

        public void RenameLayer(object parameter)
        {
            Owner.BitmapManager.ActiveDocument.Layers[(int)parameter].IsRenaming = true;
        }

        public void MoveLayerToFront(object parameter)
        {
            int oldIndex = (int)parameter;
            Owner.BitmapManager.ActiveDocument.Layers.Move(oldIndex, oldIndex + 1);
            if (Owner.BitmapManager.ActiveDocument.ActiveLayerIndex == oldIndex)
            {
                Owner.BitmapManager.SetActiveLayer(oldIndex + 1);
            }
        }

        public void MoveLayerToBack(object parameter)
        {
            int oldIndex = (int)parameter;
            Owner.BitmapManager.ActiveDocument.Layers.Move(oldIndex, oldIndex - 1);
            if (Owner.BitmapManager.ActiveDocument.ActiveLayerIndex == oldIndex)
            {
                Owner.BitmapManager.SetActiveLayer(oldIndex - 1);
            }
        }

        public bool CanMoveToFront(object property)
        {
            return Owner.DocumentIsNotNull(null) && Owner.BitmapManager.ActiveDocument.Layers.Count - 1 > (int)property;
        }

        public bool CanMoveToBack(object property)
        {
            return (int)property > 0;
        }
    }
}