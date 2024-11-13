using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using Drawie.Numerics;
using PixiEditor.UI.Common.Fonts;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.C)]
internal class VectorEllipseToolViewModel : ShapeTool, IVectorEllipseToolHandler
{
    private string defaultActionDisplay = "ELLIPSE_TOOL_ACTION_DISPLAY_DEFAULT";
    public override string ToolNameLocalizationKey => "ELLIPSE_TOOL";

    public VectorEllipseToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
    }

    // This doesn't include a Vector layer because it is designed to create new layer each use
    public override Type[]? SupportedLayerTypes { get; } = [];
    public override LocalizedString Tooltip => new LocalizedString("ELLIPSE_TOOL_TOOLTIP", Shortcut);
    public bool DrawCircle { get; private set; }

    public override string DefaultIcon => PixiPerfectIcons.Circle;

    public override Type LayerTypeToCreateOnEmptyUse { get; } = typeof(VectorLayerNode);

    public string? DefaultNewLayerName { get; } = new LocalizedString("NEW_ELLIPSE_LAYER_NAME");

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseVectorEllipseTool();
    }

    public override void OnSelected(bool restoring)
    {
        if (restoring) return;
        
        var document = ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument;
        var layer = document.SelectedStructureMember;
        if (layer is IVectorLayerHandler vectorLayer && 
            vectorLayer.GetShapeData(document.AnimationDataViewModel.ActiveFrameTime) is IReadOnlyEllipseData)
        {
            ShapeCorners corners = vectorLayer.TransformationCorners;
            document.TransformViewModel.ShowTransform(
                DocumentTransformMode.Scale_Rotate_Shear_NoPerspective, false, corners, false);
        }

        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseVectorEllipseTool();
    }

    protected override void OnSelectedLayersChanged(IStructureMemberHandler[] layers)
    {
        OnDeselecting(false);
        OnSelected(false);
    }
}
