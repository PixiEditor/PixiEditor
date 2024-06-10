using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;
using PixiEditor.AvaloniaUI.Views.Overlays.BrushShapeOverlay;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.UI.Common.Fonts;

namespace PixiEditor.AvaloniaUI.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.N)]
internal class RotateViewportToolViewModel : ToolViewModel
{
    public override string ToolNameLocalizationKey => "ROTATE_VIEWPORT_TOOL";
    public override BrushShape BrushShape => BrushShape.Hidden;
    public override bool HideHighlight => true;
    public override bool StopsLinkedToolOnUse => false;
    public override LocalizedString Tooltip => new LocalizedString("ROTATE_VIEWPORT_TOOLTIP", Shortcut);

    public override string Icon => PixiPerfectIcons.RotateView;

    public RotateViewportToolViewModel()
    {
    }

    public override void OnSelected()
    {
        ActionDisplay = new LocalizedString("ROTATE_VIEWPORT_ACTION_DISPLAY");
    }
}
