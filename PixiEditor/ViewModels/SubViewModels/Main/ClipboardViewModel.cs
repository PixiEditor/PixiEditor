using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Commands.Search;
using PixiEditor.Models.Controllers;
using SkiaSharp;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    [Command.Group("PixiEditor.Clipboard", "Clipboard")]
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

        [Command.Basic("PixiEditor.Clipboard.PasteColor", "Paste color", "Paste color from clipboard", CanExecute = "PixiEditor.Clipboard.CanPasteColor", IconEvaluator = "PixiEditor.Clipboard.PasteColorIcon")]
        public void PasteColor()
        {
            Owner.ColorsSubViewModel.PrimaryColor = SKColor.Parse(Clipboard.GetText().Trim());
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

        [Evaluator.CanExecute("PixiEditor.Clipboard.CanPasteColor")]
        public static bool CanPasteColor() => Regex.IsMatch(Clipboard.GetText().Trim(), "^#?([a-fA-F0-9]{8}|[a-fA-F0-9]{6}|[a-fA-F0-9]{3})$");

        [Evaluator.Icon("PixiEditor.Clipboard.PasteColorIcon")]
        public static ImageSource GetPasteColorIcon()
        {
            Color color;

            if (CanPasteColor())
            {
                color = SKColor.Parse(Clipboard.GetText().Trim()).ToColor();
            }
            else
            {
                color = Colors.Transparent;
            }

            return ColorSearchResult.GetIcon(color.ToSKColor());
        }
    }
}