using Avalonia.Input;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.H, Transient = Key.Space)]
internal class MoveViewportToolViewModel : ToolViewModel
{
    public override string ToolNameLocalizationKey => "MOVE_VIEWPORT_TOOL";
    public override BrushShape BrushShape => BrushShape.Hidden;
    public override bool HideHighlight => true;
    public override LocalizedString Tooltip => new LocalizedString("MOVE_VIEWPORT_TOOLTIP", Shortcut);

    public override string Icon => PixiPerfectIcons.MoveView;

    public override bool StopsLinkedToolOnUse => false;

    public MoveViewportToolViewModel()
    {
        Cursor = new Cursor(StandardCursorType.SizeAll);
    }

    public override void OnSelected()
    {
        ActionDisplay = new LocalizedString("MOVE_VIEWPORT_ACTION_DISPLAY");
    }
}
