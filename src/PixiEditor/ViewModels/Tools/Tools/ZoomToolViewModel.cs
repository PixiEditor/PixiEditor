using Avalonia.Input;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.Tools;

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
    public override BrushShape FinalBrushShape => BrushShape.Hidden;
    public override Type[]? SupportedLayerTypes { get; } = null;

    public override bool StopsLinkedToolOnUse => false;

    public override string DefaultIcon => PixiPerfectIcons.ZoomIn;

    public ZoomToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
    }

    public override Type LayerTypeToCreateOnEmptyUse { get; } = null;
    public override bool HideHighlight => true;

    public override LocalizedString Tooltip => new LocalizedString("ZOOM_TOOL_TOOLTIP", Shortcut);

    public override void KeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown, Key argsKey)
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
