using PixiEditor.Helpers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Layers;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class LayersViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand SetActiveLayerCommand { get; set; }

        public RelayCommand NewLayerCommand { get; set; }

        public RelayCommand NewTemplateLayerCommand { get; set; }

        public RelayCommand DeleteLayerCommand { get; set; }

        public RelayCommand RenameLayerCommand { get; set; }

        public RelayCommand MoveToBackCommand { get; set; }

        public RelayCommand MoveToFrontCommand { get; set; }

        public RelayCommand MergeWithAboveCommand { get; set; }

        public RelayCommand MergeWithBelowCommand { get; set; }

        public LayersViewModel(ViewModelMain owner)
            : base(owner)
        {
            SetActiveLayerCommand = new RelayCommand(SetActiveLayer);
            NewLayerCommand = new RelayCommand(NewLayer, CanCreateNewLayer);
            NewTemplateLayerCommand = new RelayCommand(NewTemplateLayer, CanCreateNewLayer);
            DeleteLayerCommand = new RelayCommand(DeleteLayer, CanDeleteLayer);
            MoveToBackCommand = new RelayCommand(MoveLayerToBack, CanMoveToBack);
            MoveToFrontCommand = new RelayCommand(MoveLayerToFront, CanMoveToFront);
            RenameLayerCommand = new RelayCommand(RenameLayer);
            MergeWithAboveCommand = new RelayCommand(MergeWithAbove, CanMergeWithAbove);
            MergeWithBelowCommand = new RelayCommand(MergeWithBelow, CanMergeWithBelow);
        }

        public void NewTemplateLayer(object parameter)
        {
            ImportFileDialog dialog = new ImportFileDialog();

            if (dialog.ShowDialog())
            {
                TemplateLayer template = new TemplateLayer(dialog.FilePath, Owner.BitmapManager.ActiveDocument.Width, Owner.BitmapManager.ActiveDocument.Height);
                Owner.BitmapManager.ActiveDocument.DocumentSizeChanged += template.DocumentSizeChanged;
                Owner.BitmapManager.ActiveDocument.Layers.Add(template);
            }
        }

        public void NewLayer(object parameter)
        {
            Owner.BitmapManager.ActiveDocument.AddNewLayer($"New Layer {Owner.BitmapManager.ActiveDocument.Layers.Count}");
        }

        public bool CanCreateNewLayer(object parameter)
        {
            return Owner.BitmapManager.ActiveDocument != null && Owner.BitmapManager.ActiveDocument.Layers.Count > 0;
        }

        public void SetActiveLayer(object parameter)
        {
            Owner.BitmapManager.ActiveDocument.SetActiveLayer((int)parameter);
        }

        public void DeleteLayer(object parameter)
        {
            Owner.BitmapManager.ActiveDocument.RemoveLayer((int)parameter);
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
                index = Owner.BitmapManager.ActiveDocument.ActiveLayerIndex;
            }

            Owner.BitmapManager.ActiveDocument.Layers[index.Value].IsRenaming = true;
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
            return Owner.DocumentIsNotNull(null) && Owner.BitmapManager.ActiveDocument.Layers.Count - 1 > (int)property;
        }

        public bool CanMoveToBack(object property)
        {
            return (int)property > 0;
        }

        public void MergeWithAbove(object parameter)
        {
            int index = (int)parameter;
            Owner.BitmapManager.ActiveDocument.MergeLayers(index, index + 1, false);
        }

        public void MergeWithBelow(object parameter)
        {
            int index = (int)parameter;
            Owner.BitmapManager.ActiveDocument.MergeLayers(index - 1, index, true);
        }

        public bool CanMergeWithAbove(object propery)
        {
            int index = (int)propery;
            return Owner.DocumentIsNotNull(null) && index != Owner.BitmapManager.ActiveDocument.Layers.Count - 1;
        }

        public bool CanMergeWithBelow(object propery)
        {
            int index = (int)propery;
            return Owner.DocumentIsNotNull(null) && index != 0;
        }
    }
}