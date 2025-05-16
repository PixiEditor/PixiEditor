using System.ComponentModel;
using Avalonia.Input;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.P)]
internal class VectorPathToolViewModel : ShapeTool, IVectorPathToolHandler
{
    public const string NewLayerKey = "DEFAULT_PATH_LAYER_NAME";
    public override string ToolNameLocalizationKey => "PATH_TOOL";
    public override Type[]? SupportedLayerTypes { get; } = [typeof(IVectorLayerHandler)];
    public override Type LayerTypeToCreateOnEmptyUse { get; } = typeof(VectorLayerNode);
    public override LocalizedString Tooltip => new LocalizedString("PATH_TOOL_TOOLTIP", Shortcut);

    public string? DefaultNewLayerName => new LocalizedString(NewLayerKey);

    public override string DefaultIcon => PixiPerfectIcons.VectorPen;
    public override bool StopsLinkedToolOnUse => false;
    public override bool IsErasable => false;

    private bool isActivated;

    private LocalizedString actionDisplayDefault;
    private LocalizedString actionDisplayCtrl;
    private LocalizedString actionDisplayAlt;
    private LocalizedString actionDisplayShift;
    private LocalizedString actionDisplayCtrlShift;

    [Settings.Enum("FILL_MODE", VectorPathFillType.Winding)]
    public VectorPathFillType FillMode
    {
        get => GetValue<VectorPathFillType>();
    }

    [Settings.Enum("STROKE_CAP", StrokeCap.Round)]
    public StrokeCap StrokeLineCap
    {
        get => GetValue<StrokeCap>();
    }

    [Settings.Enum("STROKE_JOIN", StrokeJoin.Round)]
    public StrokeJoin StrokeLineJoin
    {
        get => GetValue<StrokeJoin>();
    }

    public VectorPathToolViewModel()
    {
        Toolbar = ToolbarFactory.Create<VectorPathToolViewModel, FillableShapeToolbar>(this);
        var fillSetting = Toolbar.GetSetting(nameof(FillableShapeToolbar.Fill));
        if (fillSetting != null)
        {
            fillSetting.Value = false;
        }

        var strokeSetting = Toolbar.GetSetting(nameof(ShapeToolbar.ToolSize));
        if (strokeSetting != null)
        {
            strokeSetting.Value = 1d;
        }
        
        actionDisplayDefault = new LocalizedString("PATH_TOOL_ACTION_DISPLAY");
        actionDisplayCtrl = new LocalizedString("PATH_TOOL_ACTION_DISPLAY_CTRL");
        actionDisplayAlt = new LocalizedString("PATH_TOOL_ACTION_DISPLAY_ALT");
        actionDisplayShift = new LocalizedString("PATH_TOOL_ACTION_DISPLAY_SHIFT");
        actionDisplayCtrlShift = new LocalizedString("PATH_TOOL_ACTION_DISPLAY_CTRL_SHIFT");
    }

    public override void UseTool(VecD pos)
    {
        var doc =
            ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument;

        if (doc is null) return;

        if (!doc.PathOverlayViewModel.IsActive)
        {
            doc?.Tools.UseVectorPathTool();
            isActivated = true;
        }
    }

    public override void KeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown, Key argsKey)
    {
        if (ctrlIsDown)
        {
            if (shiftIsDown)
            {
                ActionDisplay = actionDisplayCtrlShift;
            }
            else
            {
                ActionDisplay = actionDisplayCtrl;
            }
        }
        else if (altIsDown)
        {
            ActionDisplay = actionDisplayAlt;
        }
        else if (shiftIsDown)
        {
            ActionDisplay = actionDisplayShift;
        }
        else
        {
            ActionDisplay = actionDisplayDefault;
        }
    }
    
    protected override void OnSelected(bool restoring)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseVectorPathTool();
        isActivated = true;
    }
    
    protected override void OnDeselecting(bool transient)
    {
        if (!transient)
        {
            ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Operations.TryStopToolLinkedExecutor();
            isActivated = false;
        }
    }

    public override void OnPostUndoInlet()
    {
        if (isActivated)
        {
            OnToolSelected(false);
        }
    }

    public override void OnPostRedoInlet()
    {
        if (isActivated)
        {
            OnToolSelected(false);
        }
    }

    protected override void OnSelectedLayersChanged(IStructureMemberHandler[] layers)
    {
        OnDeselecting(false);
        OnToolSelected(false);
    }
}

enum VectorPathFillType
{
    [Description("FILL_TYPE_WINDING")]
    
    Winding,
    [Description("FILL_TYPE_EVEN_ODD")]
    EvenOdd,
    
    [Description("FILL_TYPE_INVERSE_WINDING")]
    InverseWinding,
    
    [Description("FILL_TYPE_INVERSE_EVEN_ODD")]
    InverseEvenOdd
}
