using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using PixiEditor.Helpers.Extensions;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Helpers;
using PixiEditor.Models.Clipboard;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Attributes.Evaluators;
using PixiEditor.Models.Commands.Search;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.IO;
using PixiEditor.Numerics;
using PixiEditor.UI.Common.Fonts;

namespace PixiEditor.ViewModels.SubViewModels;
#nullable enable
[Command.Group("PixiEditor.Clipboard", "CLIPBOARD")]
internal class ClipboardViewModel : SubViewModel<ViewModelMain>
{
    public ClipboardViewModel(ViewModelMain owner)
        : base(owner)
    {
        Application.Current.ForDesktopMainWindow((mainWindow) =>
        {
            ClipboardController.Initialize(mainWindow.Clipboard);
        });
    }

    [Command.Basic("PixiEditor.Clipboard.Cut", "CUT", "CUT_DESCRIPTIVE", CanExecute = "PixiEditor.Selection.IsNotEmpty", Key = Key.X, Modifiers = KeyModifiers.Control,
        MenuItemPath = "EDIT/CUT", MenuItemOrder = 2, Icon = PixiPerfectIcons.Scissors)]
    public async Task Cut()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        await Copy();
        doc.Operations.DeleteSelectedPixels(doc.AnimationDataViewModel.ActiveFrameBindable, true);
    }

    [Command.Basic("PixiEditor.Clipboard.Paste", false, "PASTE", "PASTE_DESCRIPTIVE", CanExecute = "PixiEditor.Clipboard.CanPaste", Key = Key.V, Modifiers = KeyModifiers.Shift,
        MenuItemPath = "EDIT/PASTE", MenuItemOrder = 4, Icon = PixiPerfectIcons.Paste)]
    [Command.Basic("PixiEditor.Clipboard.PasteAsNewLayer", true, "PASTE_AS_NEW_LAYER", "PASTE_AS_NEW_LAYER_DESCRIPTIVE", CanExecute = "PixiEditor.Clipboard.CanPaste", Key = Key.V, Modifiers = KeyModifiers.Control,
        Icon = PixiPerfectIcons.PasteAsNewLayer)]
    public void Paste(bool pasteAsNewLayer)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null) 
            return;
        ClipboardController.TryPasteFromClipboard(Owner.DocumentManagerSubViewModel.ActiveDocument, pasteAsNewLayer);
    }
    
    [Command.Basic("PixiEditor.Clipboard.PasteReferenceLayer", "PASTE_REFERENCE_LAYER", "PASTE_REFERENCE_LAYER_DESCRIPTIVE", CanExecute = "PixiEditor.Clipboard.CanPaste",
        Icon = PixiPerfectIcons.PasteReferenceLayer)]
    public async Task PasteReferenceLayer(IDataObject data)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;

        DataImage imageData = (data == null ? await ClipboardController.GetImagesFromClipboard() : ClipboardController.GetImage(data)).First();
        using var surface = imageData.image;

        // TODO: Implement this
        /*var bitmap = imageData.image.ToWriteableBitmap();

        byte[] pixels = bitmap.ExtractPixels();

        doc.Operations.ImportReferenceLayer(
            pixels.ToImmutableArray(),
            imageData.image.Size);*/

        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow!.Activate();
        }
    }
    
    [Command.Internal("PixiEditor.Clipboard.PasteReferenceLayerFromPath")]
    public void PasteReferenceLayer(string path)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;

        // TODO: Exception handling would probably be good
        var bitmap = Importer.GetPreviewSurface(path);
        byte[] pixels = bitmap.ToWriteableBitmap().ExtractPixels();

        doc.Operations.ImportReferenceLayer(
            pixels.ToImmutableArray(),
            new VecI(bitmap.Size.X, bitmap.Size.Y));
    }

    [Command.Basic("PixiEditor.Clipboard.PasteColor", false, "PASTE_COLOR", "PASTE_COLOR_DESCRIPTIVE", CanExecute = "PixiEditor.Clipboard.CanPasteColor", IconEvaluator = "PixiEditor.Clipboard.PasteColorIcon")]
    [Command.Basic("PixiEditor.Clipboard.PasteColorAsSecondary", true, "PASTE_COLOR_SECONDARY", "PASTE_COLOR_SECONDARY_DESCRIPTIVE", CanExecute = "PixiEditor.Clipboard.CanPasteColor", IconEvaluator = "PixiEditor.Clipboard.PasteColorIcon")]
    public async Task PasteColor(bool secondary)
    {
        if (!ColorHelper.ParseAnyFormat((await ClipboardController.Clipboard.GetTextAsync())?.Trim() ?? string.Empty, out var result))
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

    [Command.Basic("PixiEditor.Clipboard.Copy", "COPY", "COPY_DESCRIPTIVE", CanExecute = "PixiEditor.Selection.IsNotEmpty", Key = Key.C, Modifiers = KeyModifiers.Control,
        MenuItemPath = "EDIT/COPY", MenuItemOrder = 3, Icon = PixiPerfectIcons.Copy)]
    public async Task Copy()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        await ClipboardController.CopyToClipboard(doc);
    }

    [Command.Basic("PixiEditor.Clipboard.CopyPrimaryColorAsHex", CopyColor.PrimaryHEX, "COPY_COLOR_HEX", "COPY_COLOR_HEX_DESCRIPTIVE", IconEvaluator = "PixiEditor.Clipboard.CopyColorIcon")]
    [Command.Basic("PixiEditor.Clipboard.CopyPrimaryColorAsRgb", CopyColor.PrimaryRGB, "COPY_COLOR_RGB", "COPY_COLOR_RGB_DESCRIPTIVE", IconEvaluator = "PixiEditor.Clipboard.CopyColorIcon")]
    [Command.Basic("PixiEditor.Clipboard.CopySecondaryColorAsHex", CopyColor.SecondaryHEX, "COPY_COLOR_SECONDARY_HEX", "COPY_COLOR_SECONDARY_HEX_DESCRIPTIVE", IconEvaluator = "PixiEditor.Clipboard.CopyColorIcon")]
    [Command.Basic("PixiEditor.Clipboard.CopySecondaryColorAsRgb", CopyColor.SecondardRGB, "COPY_COLOR_SECONDARY_RGB", "COPY_COLOR_SECONDARY_RGB_DESCRIPTIVE", IconEvaluator = "PixiEditor.Clipboard.CopyColorIcon")]
    [Command.Filter("PixiEditor.Clipboard.CopyColorToClipboard", "COPY_COLOR_TO_CLIPBOARD", "COPY_COLOR", Key = Key.C, Modifiers = KeyModifiers.Shift)]
    public async Task CopyColorAsHex(CopyColor color)
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

        await ClipboardController.Clipboard.SetTextAsync(text);
    }

    [Evaluator.CanExecute("PixiEditor.Clipboard.CanPaste")]
    public bool CanPaste(object parameter)
    {
        return Owner.DocumentIsNotNull(null) && parameter is IDataObject data ? ClipboardController.IsImage(data) : ClipboardController.IsImageInClipboard().Result;
    }

    [Evaluator.CanExecute("PixiEditor.Clipboard.CanPasteColor")]
    public static async Task<bool> CanPasteColor() => ColorHelper.ParseAnyFormat((await ClipboardController.Clipboard.GetTextAsync())?.Trim() ?? string.Empty, out _);

    [Evaluator.Icon("PixiEditor.Clipboard.PasteColorIcon")]
    public static async Task<IImage> GetPasteColorIcon()
    {
        Color color;

        color = ColorHelper.ParseAnyFormat((await ClipboardController.Clipboard.GetTextAsync())?.Trim() ?? string.Empty, out var result) ? result.Value.ToOpaqueMediaColor() : Colors.Transparent;

        return ColorSearchResult.GetIcon(color.ToOpaqueColor());
    }

    [Evaluator.Icon("PixiEditor.Clipboard.CopyColorIcon")]
    public IImage GetCopyColorIcon(object data)
    {
        if (data is CopyColor color)
        {
        }
        else if (data is Models.Commands.Commands.Command.BasicCommand command)
        {
            color = (CopyColor)command.Parameter;
        }
        else if (data is Models.Commands.Search.CommandSearchResult result)
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
