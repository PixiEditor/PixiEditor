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

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.C)]
internal class VectorEllipseToolViewModel : ShapeTool, IVectorEllipseToolHandler
{
    public const string NewLayerKey = "NEW_ELLIPSE_LAYER_NAME";
    private string defaultActionDisplay = "ELLIPSE_TOOL_ACTION_DISPLAY_DEFAULT";
    public override string ToolNameLocalizationKey => "ELLIPSE_TOOL";

    public override bool IsErasable => false;

    public VectorEllipseToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
    }

    // This doesn't include a Vector layer because it is designed to create new layer each use
    public override Type[]? SupportedLayerTypes { get; } = [];
    public override LocalizedString Tooltip => new LocalizedString("ELLIPSE_TOOL_TOOLTIP", Shortcut);

    public override string DefaultIcon => PixiPerfectIcons.Circle;

    public override Type LayerTypeToCreateOnEmptyUse { get; } = typeof(VectorLayerNode);

    public string? DefaultNewLayerName { get; } = new LocalizedString(NewLayerKey);
    

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseVectorEllipseTool();
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

    protected override void OnSelected(bool restoring)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseVectorEllipseTool();
    }

    public override void OnPostUndoInlet()
    {
        if (IsActive)
        {
            OnSelected(false);
        }
    }

    public override void OnPostRedoInlet()
    {
        if (IsActive)
        {
            OnSelected(false);
        }
    }

    protected override void OnSelectedLayersChanged(IStructureMemberHandler[] layers)
    {
        OnDeselecting(false);
        OnSelected(false);
    }
}
