using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.Models.Commands.Attributes.Commands;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.O, Transient = Key.LeftAlt)]
internal class ColorPickerToolViewModel : ToolViewModel
{
    private readonly string defaultActionDisplay = "Click to pick colors. Hold Ctrl to hide the canvas. Hold Shift to hide the reference layer";

    public ColorPickerToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
    }

    public override bool HideHighlight => true;

    public override string Tooltip => $"Picks the primary color from the canvas. ({Shortcut})";

    public override void OnLeftMouseButtonDown(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.UseColorPickerTool();
    }

    public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        /*
        if (!IsActive)
        {
            _bitmapManager.HideReferenceLayer = false;
            _bitmapManager.OnlyReferenceLayer = false;
            return;
        }

        if (ctrlIsDown)
        {
            _bitmapManager.HideReferenceLayer = false;
            _bitmapManager.OnlyReferenceLayer = true;
            ActionDisplay = "Click to pick colors from the reference layer.";
        }
        else if (shiftIsDown)
        {
            _bitmapManager.HideReferenceLayer = true;
            _bitmapManager.OnlyReferenceLayer = false;
            ActionDisplay = "Click to pick colors from the canvas.";
            return;
        }
        else
        {
            _bitmapManager.HideReferenceLayer = false;
            _bitmapManager.OnlyReferenceLayer = false;
            ActionDisplay = defaultActionDisplay;
        }*/
    }
}
