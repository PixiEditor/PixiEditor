using System.Windows.Input;
using Avalonia.Input;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Localization;
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

    private string defaultActionDisplay = new LocalizedString("ZOOM_TOOL_ACTION_DISPLAY_DEFAULT");

    public override string ToolNameLocalizationKey => "ZOOM_TOOL";
    public override BrushShape BrushShape => BrushShape.Hidden;

    public ZoomToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
    }

    public override bool HideHighlight => true;

    public override LocalizedString Tooltip => new LocalizedString("ZOOM_TOOL_TOOLTIP", Shortcut);

    public override void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (ctrlIsDown)
        {
            ActionDisplay = new LocalizedString("ZOOM_TOOL_ACTION_DISPLAY_CTRL");
            ZoomOutOnClick = true;
        }
        else
        {
            ActionDisplay = defaultActionDisplay;
            ZoomOutOnClick = false;
        }
    }
}
