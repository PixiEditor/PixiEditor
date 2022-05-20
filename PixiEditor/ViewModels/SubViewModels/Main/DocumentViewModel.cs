using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using System.Windows.Input;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    [Command.Group("PixiEditor.Document", "Image")]
    public class DocumentViewModel : SubViewModel<ViewModelMain>
    {
        public const string ConfirmationDialogTitle = "Unsaved changes";
        public const string ConfirmationDialogMessage = "The document has been modified. Do you want to save changes?";

        public DocumentViewModel(ViewModelMain owner)
            : base(owner)
        {
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

        [Command.Basic("PixiEditor.Document.ClipCanvas", "Clip Canvas", "Clip Canvas", CanExecute = "PixiEditor.HasDocument")]
        public void ClipCanvas()
        {
            Owner.BitmapManager.ActiveDocument?.ClipCanvas();
        }

        public void RequestCloseDocument(Document document)
        {
            if (!document.ChangesSaved)
            {
                ConfirmationType result = ConfirmationDialog.Show(ConfirmationDialogMessage, ConfirmationDialogTitle);
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

        [Command.Basic("PixiEditor.Document.DeletePixels", "Delete pixels", "Delete selected pixels", CanExecute = "PixiEditor.Selection.IsNotEmpty", Key = Key.Delete, Icon = "Tools/EraserImage.png")]
        public void DeletePixels()
        {
            var doc = Owner.BitmapManager.ActiveDocument;
            Owner.BitmapManager.BitmapOperations.DeletePixels(
                doc.Layers.Where(x => x.IsActive && doc.GetFinalLayerIsVisible(x)).ToArray(),
                doc.ActiveSelection.SelectedPoints.ToArray());
        }

        [Command.Basic("PixiEditor.Document.ResizeDocument", false, "Resize Document", "Resize Document", CanExecute = "PixiEditor.HasDocument", Key = Key.I, Modifiers = ModifierKeys.Control | ModifierKeys.Shift)]
        [Command.Basic("PixiEditor.Document.ResizeCanvas", true, "Resize Canvas", "Resize Canvas", CanExecute = "PixiEditor.HasDocument", Key = Key.C, Modifiers = ModifierKeys.Control | ModifierKeys.Shift)]
        public void OpenResizePopup(bool canvas)
        {
            ResizeDocumentDialog dialog = new ResizeDocumentDialog(
                Owner.BitmapManager.ActiveDocument.Width,
                Owner.BitmapManager.ActiveDocument.Height,
                canvas);
            if (dialog.ShowDialog())
            {
                if (canvas)
                {
                    Owner.BitmapManager.ActiveDocument.ResizeCanvas(dialog.Width, dialog.Height, dialog.ResizeAnchor);
                }
                else
                {
                    Owner.BitmapManager.ActiveDocument.Resize(dialog.Width, dialog.Height);
                }
            }
        }

        [Command.Basic("PixiEditor.Document.CenterContent", "Center Content", "Center Content", CanExecute = "PixiEditor.HasDocument")]
        public void CenterContent()
        {
            Owner.BitmapManager.ActiveDocument.CenterContent();
        }
    }
}
