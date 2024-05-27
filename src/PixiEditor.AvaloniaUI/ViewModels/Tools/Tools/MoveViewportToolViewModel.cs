using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;
using PixiEditor.AvaloniaUI.Views.Overlays.BrushShapeOverlay;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.H, Transient = Key.Space)]
internal class MoveViewportToolViewModel : ToolViewModel
{
    public override string ToolNameLocalizationKey => "MOVE_VIEWPORT_TOOL";
    public override BrushShape BrushShape => BrushShape.Hidden;
    public override bool HideHighlight => true;
    public override LocalizedString Tooltip => new LocalizedString("MOVE_VIEWPORT_TOOLTIP", Shortcut);

    public override string IconKey => "icon-move-view";

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
