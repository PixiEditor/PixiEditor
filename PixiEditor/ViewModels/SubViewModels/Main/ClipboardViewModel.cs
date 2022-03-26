using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Controllers;
using System.Windows.Input;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class ClipboardViewModel : SubViewModel<ViewModelMain>
    {
        public ClipboardViewModel(ViewModelMain owner)
            : base(owner)
        {
        }

        [Command.Basic("PixiEditor.Clipboard.Duplicate", "Duplicate", "Duplicate selected area/layer", CanExecute = "PixiEditor.HasDocument", Key = Key.J, Modifiers = ModifierKeys.Control)]
        public void Duplicate()
        {
            Copy();
            Paste();
        }

        [Command.Basic("PixiEditor.Clipboard.Cut", "Cut", "Cut selected area/layer", CanExecute = "PixiEditor.HasDocument", Key = Key.X, Modifiers = ModifierKeys.Control)]
        public void Cut()
        {
            Copy();
            Owner.BitmapManager.BitmapOperations.DeletePixels(
                new[] { Owner.BitmapManager.ActiveDocument.ActiveLayer },
                Owner.BitmapManager.ActiveDocument.ActiveSelection.SelectedPoints.ToArray());
        }

        [Command.Basic("PixiEditor.Clipboard.Paste", "Paste", "Paste from clipboard", CanExecute = "PixiEditor.Clipboard.CanPaste", Key = Key.V, Modifiers = ModifierKeys.Control)]
        public void Paste()
        {
            if (Owner.BitmapManager.ActiveDocument == null) return;
            ClipboardController.PasteFromClipboard(Owner.BitmapManager.ActiveDocument);
        }

        [Command.Basic("PixiEditor.Clipboard.Copy", "Copy", "Copy to clipboard", CanExecute = "PixiEditor.HasDocument", Key = Key.C, Modifiers = ModifierKeys.Control)]
        public void Copy()
        {
            ClipboardController.CopyToClipboard(Owner.BitmapManager.ActiveDocument);
        }

        [Evaluator.CanExecute("PixiEditor.Clipboard.CanPaste")]
        public bool CanPaste()
        {
            return Owner.DocumentIsNotNull(null) && ClipboardController.IsImageInClipboard();
        }
    }
}