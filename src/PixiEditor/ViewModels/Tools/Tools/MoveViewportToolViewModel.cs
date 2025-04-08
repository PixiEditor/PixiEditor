using Avalonia.Input;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.H, Transient = Key.Space, TransientImmediate = true)]
internal class MoveViewportToolViewModel : ToolViewModel
{
    public override string ToolNameLocalizationKey => "MOVE_VIEWPORT_TOOL";
    public override BrushShape FinalBrushShape => BrushShape.Hidden;
    public override Type[]? SupportedLayerTypes { get; } = null;
    public override Type LayerTypeToCreateOnEmptyUse { get; } = null;
    public override bool HideHighlight => true;
    public override LocalizedString Tooltip => new LocalizedString("MOVE_VIEWPORT_TOOLTIP", Shortcut);

    public override string DefaultIcon => PixiPerfectIcons.MoveView;

    public override bool StopsLinkedToolOnUse => false;

    public MoveViewportToolViewModel()
    {
        Cursor = new Cursor(StandardCursorType.SizeAll);
    }

    protected override void OnSelected(bool restoring)
    {
        if (ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument == null)
            return;

        ActionDisplay = new LocalizedString("MOVE_VIEWPORT_ACTION_DISPLAY");
        ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument.SuppressAllOverlayEvents(ToolName);
    }

    protected override void OnDeselecting(bool transient)
    {
        if (ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument == null)
            return;

        base.OnDeselecting(transient);
        ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument.RestoreAllOverlayEvents(ToolName);
    }
}
