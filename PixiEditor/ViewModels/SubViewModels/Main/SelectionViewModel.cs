using PixiEditor.Helpers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Tools.Tools;
using System.Windows;

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
            var doc = Owner.BitmapManager.ActiveDocument;
            Int32Rect area = new(0, 0, doc.Width, doc.Height);
            doc.ActiveSelection.SetSelectionWithUndo(area, false, SelectionType.New);
        }

        public void Deselect(object parameter)
        {
            Owner.BitmapManager.ActiveDocument.ActiveSelection?.ClearWithUndo();
        }

        public bool SelectionIsNotEmpty(object property)
        {
            var empty = Owner.BitmapManager.ActiveDocument?.ActiveSelection.isEmpty;
            return empty != null && !empty.Value;
        }

        private bool CanSelectAll(object property)
        {
            return Owner.BitmapManager.ActiveDocument != null && Owner.BitmapManager.ActiveDocument.Layers.Count > 0;
        }
    }
}
