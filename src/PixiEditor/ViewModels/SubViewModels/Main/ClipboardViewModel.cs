using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Controllers;

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

    [Command.Basic("PixiEditor.Clipboard.Paste", "Paste", "Paste from clipboard", CanExecute = "PixiEditor.Clipboard.CanPaste", Key = Key.V, Modifiers = ModifierKeys.Control)]
    public void Paste()
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null) 
            return;
        ClipboardController.TryPasteFromClipboard(Owner.DocumentManagerSubViewModel.ActiveDocument);
    }

    [Command.Basic("PixiEditor.Clipboard.PasteColor", "Paste color", "Paste color from clipboard", CanExecute = "PixiEditor.Clipboard.CanPasteColor", IconEvaluator = "PixiEditor.Clipboard.PasteColorIcon")]
    public void PasteColor()
    {
        if (ParseAnyFormat(Clipboard.GetText().Trim(), out var result))
        {
            Owner.ColorsSubViewModel.PrimaryColor = result.Value;
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

    [Evaluator.CanExecute("PixiEditor.Clipboard.CanPaste")]
    public bool CanPaste()
    {
        return Owner.DocumentIsNotNull(null) && ClipboardController.IsImageInClipboard();
    }

    [Evaluator.CanExecute("PixiEditor.Clipboard.CanPasteColor")]
    public static bool CanPasteColor() => ParseAnyFormat(Clipboard.GetText().Trim(), out _);

    [Evaluator.Icon("PixiEditor.Clipboard.PasteColorIcon")]
    public static ImageSource GetPasteColorIcon()
    {
        Color color;

        if (ParseAnyFormat(Clipboard.GetText().Trim(), out var result))
        {
            color = result.Value.ToOpaqueMediaColor();
        }
        else
        {
            color = Colors.Transparent;
        }

        return ColorSearchResult.GetIcon(color.ToOpaqueColor());
    }

    private static bool ParseAnyFormat(string value, [NotNullWhen(true)] out DrawingApi.Core.ColorsImpl.Color? result)
    {
        bool hex = Regex.IsMatch(Clipboard.GetText().Trim(), "^#?([a-fA-F0-9]{8}|[a-fA-F0-9]{6}|[a-fA-F0-9]{3})$");

        if (hex)
        {
            result = DrawingApi.Core.ColorsImpl.Color.Parse(Clipboard.GetText().Trim());
            return true;
        }

        var match = Regex.Match(Clipboard.GetText().Trim(), @"(?:rgba?\(?)? *(?<r>\d{1,3})(?:, *| +)(?<g>\d{1,3})(?:, *| +)(?<b>\d{1,3})(?:(?:, *| +)(?<a>\d{0,3}))?\)?");

        if (!match.Success)
        {
            result = null;
            return false;
        }

        byte r = byte.Parse(match.Groups["r"].ValueSpan);
        byte g = byte.Parse(match.Groups["g"].ValueSpan);
        byte b = byte.Parse(match.Groups["b"].ValueSpan);
        byte a = match.Groups["a"].Success ? byte.Parse(match.Groups["a"].ValueSpan) : (byte)255;

        result = new DrawingApi.Core.ColorsImpl.Color(r, g, b, a);
        return true;

    }
}
