using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using Drawie.Numerics;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.R)]
internal class VectorRectangleToolViewModel : ShapeTool, IVectorRectangleToolHandler
{
    public const string NewLayerKey = "NEW_RECTANGLE_LAYER_NAME";

    private string defaultActionDisplay = "RECTANGLE_TOOL_ACTION_DISPLAY_DEFAULT";
    public override string ToolNameLocalizationKey => "RECTANGLE_TOOL";
    public override bool IsErasable => false;
    public override Type[]? SupportedLayerTypes { get; } = [];
    public override LocalizedString Tooltip => new LocalizedString("RECTANGLE_TOOL_TOOLTIP", Shortcut);

    public override string DefaultIcon => PixiPerfectIcons.Square;

    public override Type LayerTypeToCreateOnEmptyUse { get; } = typeof(VectorLayerNode);
    public string? DefaultNewLayerName { get; } = new LocalizedString(NewLayerKey);

    private VecD cornerRadius = new VecD(0, 0);

    [Settings.Percent("RADIUS", 0, ExposedByDefault = true, Min = 0)]
    public float CornerRadius
    {
        get
        {
            return GetValue<float>();
        }
        set
        {
            SetValue(value);
        }
    }

    public VectorRectangleToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
        Toolbar = ToolbarFactory.Create<VectorRectangleToolViewModel, FillableShapeToolbar>(this);
    }

    public override void KeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown, Key argsKey)
    {
        DrawFromCenter = ctrlIsDown;

        if (shiftIsDown)
        {
            DrawEven = true;
            ActionDisplay = "RECTANGLE_TOOL_ACTION_DISPLAY_SHIFT";
        }
        else
        {
            DrawEven = false;
            ActionDisplay = defaultActionDisplay;
        }
    }

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseVectorRectangleTool();
    }

    protected override void OnSelected(bool restoring)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseVectorRectangleTool();
    }

    public override void OnPostUndoInlet()
    {
        if (IsActive)
        {
            OnToolSelected(false);
        }
    }

    public override void OnPostRedoInlet()
    {
        if (IsActive)
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
