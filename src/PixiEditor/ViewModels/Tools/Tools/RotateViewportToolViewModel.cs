using Avalonia.Input;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.N)]
internal class RotateViewportToolViewModel : ToolViewModel
{
    public override string ToolNameLocalizationKey => "ROTATE_VIEWPORT_TOOL";
    public override BrushShape BrushShape => BrushShape.Hidden;
    public override Type[]? SupportedLayerTypes { get; } = null; // null = all
    public override Type LayerTypeToCreateOnEmptyUse { get; } = null;
    public override bool HideHighlight => true;
    public override bool StopsLinkedToolOnUse => false;
    public override LocalizedString Tooltip => new LocalizedString("ROTATE_VIEWPORT_TOOLTIP", Shortcut);

    public override string Icon => PixiPerfectIcons.RotateView;

    public RotateViewportToolViewModel()
    {
    }

    public override void OnSelected(bool restoring)
    {
        ActionDisplay = new LocalizedString("ROTATE_VIEWPORT_ACTION_DISPLAY");
    }
}
