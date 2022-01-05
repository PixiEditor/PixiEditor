using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using SkiaSharp;
using System;
using System.Linq;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class DocumentViewModel : SubViewModel<ViewModelMain>
    {
        public const string ConfirmationDialogMessage = "Document was modified. Do you want to save changes?";

        public RelayCommand CenterContentCommand { get; set; }
        public RelayCommand ClipCanvasCommand { get; set; }
        public RelayCommand DeletePixelsCommand { get; set; }
        public RelayCommand OpenResizePopupCommand { get; set; }
        public RelayCommand RotateToRightCommand { get; set; }
        public RelayCommand FlipCommand { get; set; }

        public DocumentViewModel(ViewModelMain owner)
            : base(owner)
        {
            CenterContentCommand = new RelayCommand(CenterContent, Owner.DocumentIsNotNull);
            ClipCanvasCommand = new RelayCommand(ClipCanvas, Owner.DocumentIsNotNull);
            DeletePixelsCommand = new RelayCommand(DeletePixels, Owner.SelectionSubViewModel.SelectionIsNotEmpty);
            OpenResizePopupCommand = new RelayCommand(OpenResizePopup, Owner.DocumentIsNotNull);
            RotateToRightCommand = new RelayCommand(RotateDocument, Owner.DocumentIsNotNull);
            FlipCommand = new RelayCommand(FlipDocument, Owner.DocumentIsNotNull);
        }

        public void FlipDocument(object parameter)
        {
            if (parameter is "Horizontal")
            {
                Owner.BitmapManager.ActiveDocument?.FlipActiveDocument(FlipType.Horizontal);
            }
            else if (parameter is "Vertical")
            {
                Owner.BitmapManager.ActiveDocument?.FlipActiveDocument(FlipType.Vertical);
            }
        }

        public void RotateDocument(object parameter)
        {
            if (parameter is double angle)
            {
                Owner.BitmapManager.ActiveDocument?.RotateActiveDocument((float)angle);
            }
        }

        public void ClipCanvas(object parameter)
        {
            Owner.BitmapManager.ActiveDocument?.ClipCanvas();
        }

        public void RequestCloseDocument(Document document)
        {
            if (!document.ChangesSaved)
            {
                ConfirmationType result = ConfirmationDialog.Show(ConfirmationDialogMessage);
                if (result == ConfirmationType.Yes)
                {
                    Owner.FileSubViewModel.SaveDocument(false);
                    if (!document.ChangesSaved)
                        return;
                }
                else if (result == ConfirmationType.Canceled)
                {
                    return;
                }
            }

            Owner.BitmapManager.CloseDocument(document);
        }

        private void DeletePixels(object parameter)
        {
            var doc = Owner.BitmapManager.ActiveDocument;
            Owner.BitmapManager.BitmapOperations.DeletePixels(
                doc.Layers.Where(x => x.IsActive && doc.GetFinalLayerIsVisible(x)).ToArray(),
                doc.ActiveSelection.SelectedPoints.ToArray());
        }

        private void OpenResizePopup(object parameter)
        {
            bool isCanvasDialog = (string)parameter == "canvas";
            ResizeDocumentDialog dialog = new ResizeDocumentDialog(
                Owner.BitmapManager.ActiveDocument.Width,
                Owner.BitmapManager.ActiveDocument.Height,
                isCanvasDialog);
            if (dialog.ShowDialog())
            {
                if (isCanvasDialog)
                {
                    Owner.BitmapManager.ActiveDocument.ResizeCanvas(dialog.Width, dialog.Height, dialog.ResizeAnchor);
                }
                else
                {
                    Owner.BitmapManager.ActiveDocument.Resize(dialog.Width, dialog.Height);
                }
            }
        }

        private void CenterContent(object property)
        {
            Owner.BitmapManager.ActiveDocument.CenterContent();
        }
    }
}
