using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.H, Transient = Key.Space, TransientImmediate = true)]
internal class MoveViewportToolViewModel : ToolViewModel
{
    public override string ToolNameLocalizationKey => "MOVE_VIEWPORT_TOOL";
    public override Type[]? SupportedLayerTypes { get; } = null;
    public override Type LayerTypeToCreateOnEmptyUse { get; } = null;
    public override bool HideHighlight => true;
    public override LocalizedString Tooltip => new LocalizedString("MOVE_VIEWPORT_TOOLTIP", Shortcut);

    public override string DefaultIcon => PixiPerfectIcons.Hand;

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
        ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument.TransformViewModel.TransformShowStateChanged += TransformViewModel_TransformActivated;
    }

    private void TransformViewModel_TransformActivated(bool activated)
    {
        if (ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument == null)
            return;

        if (activated)
        {
            ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument.RestoreAllOverlayEvents(ToolName);
        }
        else
        {
            ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument.SuppressAllOverlayEvents(ToolName);
        }
    }

    protected override void OnDeselecting(bool transient)
    {
        if (ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument == null)
            return;

        base.OnDeselecting(transient);
        ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument.RestoreAllOverlayEvents(ToolName);
            ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument.TransformViewModel.TransformShowStateChanged -= TransformViewModel_TransformActivated;
    }
}
