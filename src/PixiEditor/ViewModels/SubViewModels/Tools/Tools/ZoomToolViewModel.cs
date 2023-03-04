using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.Z)]
internal class ZoomToolViewModel : ToolViewModel
{
    private bool zoomOutOnClick = false;
    public bool ZoomOutOnClick
    {
        get => zoomOutOnClick;
        set => SetProperty(ref zoomOutOnClick, value);
    }

    private string defaultActionDisplay = "Click and move to zoom. Click to zoom in, hold ctrl and click to zoom out.";

    public override BrushShape BrushShape => BrushShape.Hidden;

    public ZoomToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
    }

    public override bool HideHighlight => true;

    public override string Tooltip => $"Zooms viewport ({Shortcut}). Click to zoom in, hold alt and click to zoom out.";

    public override void ModiferKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (ctrlIsDown)
        {
            ActionDisplay = "Click and move to zoom. Click to zoom out, release ctrl and click to zoom in.";
            ZoomOutOnClick = true;
        }
        else
        {
            ActionDisplay = defaultActionDisplay;
            ZoomOutOnClick = false;
        }
    }
}
