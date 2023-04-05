using System.Windows.Input;
using PixiEditor.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.H, Transient = Key.Space)]
internal class MoveViewportToolViewModel : ToolViewModel
{
    public override string ToolNameLocalizationKey => "MOVE_VIEWPORT_TOOL";
    public override BrushShape BrushShape => BrushShape.Hidden;
    public override bool HideHighlight => true;
    public override LocalizedString Tooltip => new LocalizedString("MOVE_VIEWPORT_TOOLTIP", Shortcut);

    public MoveViewportToolViewModel()
    {
        Cursor = Cursors.SizeAll;
    }

    public override void OnSelected()
    {
        ActionDisplay = new LocalizedString("MOVE_VIEWPORT_ACTION_DISPLAY");
    }
}
