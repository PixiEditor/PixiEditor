using Avalonia.Input;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.P)]
internal class VectorPathToolViewModel : ShapeTool, IVectorPathToolHandler
{
    public override string ToolNameLocalizationKey => "PATH_TOOL";
    public override Type[]? SupportedLayerTypes { get; } = [typeof(IVectorLayerHandler)];
    public override Type LayerTypeToCreateOnEmptyUse { get; } = typeof(VectorLayerNode);
    public override LocalizedString Tooltip => new LocalizedString("PATH_TOOL_TOOLTIP", Shortcut);

    public string? DefaultNewLayerName => new LocalizedString("DEFAULT_PATH_LAYER_NAME");

    public override string DefaultIcon => PixiPerfectIcons.VectorPen;
    public override bool StopsLinkedToolOnUse => false;
    public override bool IsErasable => false;

    private bool isActivated;

    private LocalizedString actionDisplayDefault;
    private LocalizedString actionDisplayCtrl;
    private LocalizedString actionDisplayAlt;

    public VectorPathToolViewModel()
    {
        var fillSetting = Toolbar.GetSetting(nameof(BasicShapeToolbar.Fill));
        if (fillSetting != null)
        {
            fillSetting.Value = false;
        }
        
        actionDisplayDefault = new LocalizedString("PATH_TOOL_ACTION_DISPLAY");
        actionDisplayCtrl = new LocalizedString("PATH_TOOL_ACTION_DISPLAY_CTRL");
        actionDisplayAlt = new LocalizedString("PATH_TOOL_ACTION_DISPLAY_ALT");
    }

    public override void UseTool(VecD pos)
    {
        var doc =
            ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument;

        if (doc is null || isActivated) return;

        if (!doc.PathOverlayViewModel.IsActive)
        {
            doc?.Tools.UseVectorPathTool();
            isActivated = true;
        }
    }

    public override void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
        if (ctrlIsDown)
        {
            ActionDisplay = actionDisplayCtrl;
        }
        else if (altIsDown)
        {
            ActionDisplay = actionDisplayAlt;
        }
        else
        {
            ActionDisplay = actionDisplayDefault;
        }
    }

    public override void OnSelected(bool restoring)
    {
        if (restoring) return;

        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseVectorPathTool();
        isActivated = true;
    }

    public override void OnDeselecting(bool transient)
    {
        if (!transient)
        {
            ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Operations.TryStopToolLinkedExecutor();
            isActivated = false;
        }
    }

    protected override void OnSelectedLayersChanged(IStructureMemberHandler[] layers)
    {
        OnDeselecting(false);
        OnSelected(false);
    }
}
