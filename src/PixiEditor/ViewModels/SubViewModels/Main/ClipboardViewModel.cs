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

    [Command.Basic("PixiEditor.Clipboard.Cut", "CUT", "CUT_DESCRIPTIVE", CanExecute = "PixiEditor.Selection.IsNotEmpty", Key = Key.X, Modifiers = ModifierKeys.Control)]
    public void Cut()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        Copy();
        doc.Operations.DeleteSelectedPixels(true);
    }

    [Command.Basic("PixiEditor.Clipboard.Paste", false, "PASTE", "PASTE_DESCRIPTIVE", CanExecute = "PixiEditor.Clipboard.CanPaste", Key = Key.V, Modifiers = ModifierKeys.Shift)]
    [Command.Basic("PixiEditor.Clipboard.PasteAsNewLayer", true, "PASTE_AS_NEW_LAYER", "PASTE_AS_NEW_LAYER_DESCRIPTIVE", CanExecute = "PixiEditor.Clipboard.CanPaste", IconPath = "$PixiEditor.Clipboard.Paste", Key = Key.V, Modifiers = ModifierKeys.Control)]
    public void Paste(bool pasteAsNewLayer)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null) 
            return;
        ClipboardController.TryPasteFromClipboard(Owner.DocumentManagerSubViewModel.ActiveDocument, pasteAsNewLayer);
    }
    
    [Command.Basic("PixiEditor.Clipboard.PasteReferenceLayer", "PASTE_REFERENCE_LAYER", "PASTE_REFERENCE_LAYER_DESCRIPTIVE", CanExecute = "PixiEditor.Clipboard.CanPaste", IconPath = "Commands/PixiEditor/Clipboard/Paste.png")]
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

    [Command.Basic("PixiEditor.Clipboard.PasteColor", false, "PASTE_COLOR", "PASTE_COLOR_DESCRIPTIVE", CanExecute = "PixiEditor.Clipboard.CanPasteColor", IconEvaluator = "PixiEditor.Clipboard.PasteColorIcon")]
    [Command.Basic("PixiEditor.Clipboard.PasteColorAsSecondary", true, "PASTE_COLOR_SECONDARY", "PASTE_COLOR_SECONDARY_DESCRIPTIVE", CanExecute = "PixiEditor.Clipboard.CanPasteColor", IconEvaluator = "PixiEditor.Clipboard.PasteColorIcon")]
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

    [Command.Basic("PixiEditor.Clipboard.Copy", "COPY", "COPY_DESCRIPTIVE", CanExecute = "PixiEditor.Selection.IsNotEmpty", Key = Key.C, Modifiers = ModifierKeys.Control)]
    public void Copy()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        ClipboardController.CopyToClipboard(doc);
    }

    [Command.Basic("PixiEditor.Clipboard.CopyPrimaryColorAsHex", CopyColor.PrimaryHEX, "COPY_COLOR_HEX", "COPY_COLOR_HEX_DESCRIPTIVE", IconEvaluator = "PixiEditor.Clipboard.CopyColorIcon")]
    [Command.Basic("PixiEditor.Clipboard.CopyPrimaryColorAsRgb", CopyColor.PrimaryRGB, "COPY_COLOR_RGB", "COPY_COLOR_RGB_DESCRIPTIVE", IconEvaluator = "PixiEditor.Clipboard.CopyColorIcon")]
    [Command.Basic("PixiEditor.Clipboard.CopySecondaryColorAsHex", CopyColor.SecondaryHEX, "COPY_COLOR_SECONDARY_HEX", "COPY_COLOR_SECONDARY_HEX_DESCRIPTIVE", IconEvaluator = "PixiEditor.Clipboard.CopyColorIcon")]
    [Command.Basic("PixiEditor.Clipboard.CopySecondaryColorAsRgb", CopyColor.SecondardRGB, "COPY_COLOR_SECONDARY_RGB", "COPY_COLOR_SECONDARY_RGB_DESCRIPTIVE", IconEvaluator = "PixiEditor.Clipboard.CopyColorIcon")]
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
