using System.Collections.Immutable;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.IO;

namespace PixiEditor.ViewModels.SubViewModels.Main;
#nullable enable
[Command.Group("PixiEditor.Clipboard", "Clipboard")]
internal class ClipboardViewModel : SubViewModel<ViewModelMain>
{
    public ClipboardViewModel(ViewModelMain owner)
        : base(owner)
    {
    }

    [Command.Basic("PixiEditor.Clipboard.Cut", "Cut", "Cut selected area/layer", CanExecute = "PixiEditor.Selection.IsNotEmpty", Key = Key.X, Modifiers = ModifierKeys.Control)]
    public void Cut()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        Copy();
        doc.Operations.DeleteSelectedPixels(true);
    }

    [Command.Basic("PixiEditor.Clipboard.Paste", false, "Paste", "Paste from clipboard", CanExecute = "PixiEditor.Clipboard.CanPaste", Key = Key.V, Modifiers = ModifierKeys.Shift)]
    [Command.Basic("PixiEditor.Clipboard.PasteAsNewLayer", true, "Paste as new layer", "Paste from clipboard as new layer", CanExecute = "PixiEditor.Clipboard.CanPaste", Key = Key.V, Modifiers = ModifierKeys.Control)]
    public void Paste(bool pasteAsNewLayer)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null) 
            return;
        ClipboardController.TryPasteFromClipboard(Owner.DocumentManagerSubViewModel.ActiveDocument, pasteAsNewLayer);
    }
    
    [Command.Basic("PixiEditor.Clipboard.PasteReferenceLayer", "Paste reference layer", "Paste reference layer from clipboard", CanExecute = "PixiEditor.Clipboard.CanPaste")]
    public void PasteReferenceLayer(DataObject data)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;

        var surface = (data == null ? ClipboardController.GetImagesFromClipboard() : ClipboardController.GetImage(data)).First();
        using var image = surface.image;
        
        var bitmap = surface.image.ToWriteableBitmap();

        byte[] pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
        bitmap.CopyPixels(pixels, bitmap.PixelWidth * 4, 0);

        doc.Operations.ImportReferenceLayer(
            pixels.ToImmutableArray(),
            surface.image.Size);
    }
    
    [Command.Internal("PixiEditor.Clipboard.PasteReferenceLayerFromPath")]
    public void PasteReferenceLayer(string path)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;

        var bitmap = Importer.GetPreviewBitmap(path);
        byte[] pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
        bitmap.CopyPixels(pixels, bitmap.PixelWidth * 4, 0);

        doc.Operations.ImportReferenceLayer(
            pixels.ToImmutableArray(),
            new VecI(bitmap.PixelWidth, bitmap.PixelHeight));
    }

    [Command.Basic("PixiEditor.Clipboard.PasteColor", false, "Paste color", "Paste color from clipboard", CanExecute = "PixiEditor.Clipboard.CanPasteColor", IconEvaluator = "PixiEditor.Clipboard.PasteColorIcon")]
    [Command.Basic("PixiEditor.Clipboard.PasteColorAsSecondary", true, "Paste color as secondary", "Paste color as secondary from clipboard", CanExecute = "PixiEditor.Clipboard.CanPasteColor", IconEvaluator = "PixiEditor.Clipboard.PasteColorIcon")]
    public void PasteColor(bool secondary)
    {
        if (!ColorHelper.ParseAnyFormat(Clipboard.GetText().Trim(), out var result))
        {
            return;
        }

        if (!secondary)
        {
            Owner.ColorsSubViewModel.PrimaryColor = result.Value;
        }
        else
        {
            Owner.ColorsSubViewModel.SecondaryColor = result.Value;
        }
    }

    [Command.Basic("PixiEditor.Clipboard.Copy", "Copy", "Copy to clipboard", CanExecute = "PixiEditor.Selection.IsNotEmpty", Key = Key.C, Modifiers = ModifierKeys.Control)]
    public void Copy()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        ClipboardController.CopyToClipboard(doc);
    }

    [Command.Basic("PixiEditor.Clipboard.CopyPrimaryColorAsHex", CopyColor.PrimaryHEX, "Copy primary color (HEX)", "Copy primary color as hex code", IconEvaluator = "PixiEditor.Clipboard.CopyColorIcon")]
    [Command.Basic("PixiEditor.Clipboard.CopyPrimaryColorAsRgb", CopyColor.PrimaryRGB, "Copy primary color (RGB)", "Copy primary color as RGB code", IconEvaluator = "PixiEditor.Clipboard.CopyColorIcon")]
    [Command.Basic("PixiEditor.Clipboard.CopySecondaryColorAsHex", CopyColor.SecondaryHEX, "Copy secondary color (HEX)", "Copy secondary color as hex code", IconEvaluator = "PixiEditor.Clipboard.CopyColorIcon")]
    [Command.Basic("PixiEditor.Clipboard.CopySecondaryColorAsRgb", CopyColor.SecondardRGB, "Copy secondary color (RGB)", "Copy secondary color as RGB code", IconEvaluator = "PixiEditor.Clipboard.CopyColorIcon")]
    public void CopyColorAsHex(CopyColor color)
    {
        var targetColor = color switch
        {
            CopyColor.PrimaryHEX or CopyColor.PrimaryRGB => Owner.ColorsSubViewModel.PrimaryColor,
            _ => Owner.ColorsSubViewModel.SecondaryColor
        };

        string text = color switch
        {
            CopyColor.PrimaryHEX or CopyColor.SecondaryHEX => targetColor.A == 255
                ? $"#{targetColor.R:X2}{targetColor.G:X2}{targetColor.B:X2}"
                : targetColor.ToString(),
            _ => targetColor.A == 255
                ? $"rgb({targetColor.R},{targetColor.G},{targetColor.B})"
                : $"rgba({targetColor.R},{targetColor.G},{targetColor.B},{targetColor.A})",
        };

        Clipboard.SetText(text);
    }

    [Evaluator.CanExecute("PixiEditor.Clipboard.CanPaste")]
    public bool CanPaste()
    {
        return Owner.DocumentIsNotNull(null) && ClipboardController.IsImageInClipboard();
    }

    [Evaluator.CanExecute("PixiEditor.Clipboard.CanPasteColor")]
    public static bool CanPasteColor() => ColorHelper.ParseAnyFormat(Clipboard.GetText().Trim(), out _);

    [Evaluator.Icon("PixiEditor.Clipboard.PasteColorIcon")]
    public static ImageSource GetPasteColorIcon()
    {
        Color color;

        color = ColorHelper.ParseAnyFormat(Clipboard.GetText().Trim(), out var result) ? result.Value.ToOpaqueMediaColor() : Colors.Transparent;

        return ColorSearchResult.GetIcon(color.ToOpaqueColor());
    }

    [Evaluator.Icon("PixiEditor.Clipboard.CopyColorIcon")]
    public ImageSource GetCopyColorIcon(object data)
    {
        if (data is CopyColor color)
        {
        }
        else if (data is Models.Commands.Commands.Command.BasicCommand command)
        {
            color = (CopyColor)command.Parameter;
        }
        else if (data is CommandSearchResult result)
        {
            color = (CopyColor)((Models.Commands.Commands.Command.BasicCommand)result.Command).Parameter;
        }
        else
        {
            throw new ArgumentException("data must be of type CopyColor, BasicCommand or CommandSearchResult");
        }
        
        var targetColor = color switch
        {
            CopyColor.PrimaryHEX or CopyColor.PrimaryRGB => Owner.ColorsSubViewModel.PrimaryColor,
            _ => Owner.ColorsSubViewModel.SecondaryColor
        };

        return ColorSearchResult.GetIcon(targetColor.ToOpaqueMediaColor().ToOpaqueColor());
    }

    public enum CopyColor
    {
        PrimaryHEX,
        PrimaryRGB,
        SecondaryHEX,
        SecondardRGB
    }
}
