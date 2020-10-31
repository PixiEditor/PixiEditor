using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class ClipboardViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand CopyCommand { get; set; }
        public RelayCommand DuplicateCommand { get; set; }
        public RelayCommand CutCommand { get; set; }
        public RelayCommand PasteCommand { get; set; }

        public ClipboardViewModel(ViewModelMain owner) : base(owner)
        {
            CopyCommand = new RelayCommand(Copy, Owner.SelectionSubViewModel.SelectionIsNotEmpty);
            DuplicateCommand = new RelayCommand(Duplicate, Owner.SelectionSubViewModel.SelectionIsNotEmpty);
            CutCommand = new RelayCommand(Cut, Owner.SelectionSubViewModel.SelectionIsNotEmpty);
            PasteCommand = new RelayCommand(Paste, CanPaste);
        }

        public void Duplicate(object parameter)
        {
            Copy(null);
            Paste(null);
        }

        public void Cut(object parameter)
        {
            Copy(null);
            Owner.BitmapManager.ActiveLayer.SetPixels(
                BitmapPixelChanges.FromSingleColoredArray(Owner.SelectionSubViewModel.ActiveSelection.SelectedPoints.ToArray(),
                    Colors.Transparent));
        }

        public void Paste(object parameter)
        {
            ClipboardController.PasteFromClipboard();
        }

        private bool CanPaste(object property)
        {
            return Owner.DocumentIsNotNull(null) && ClipboardController.IsImageInClipboard();
        }

        private void Copy(object parameter)
        {
            ClipboardController.CopyToClipboard(Owner.BitmapManager.ActiveDocument.Layers.ToArray(),
                Owner.SelectionSubViewModel.ActiveSelection.SelectedPoints.ToArray(),
                Owner.BitmapManager.ActiveDocument.Width,
                Owner.BitmapManager.ActiveDocument.Height);
        }

    }
}
