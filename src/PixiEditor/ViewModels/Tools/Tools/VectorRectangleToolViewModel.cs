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

[Command.Tool(Key = Key.R)]
internal class VectorRectangleToolViewModel : ShapeTool, IVectorRectangleToolHandler
{
    private string defaultActionDisplay = "RECTANGLE_TOOL_ACTION_DISPLAY_DEFAULT";
    public override string ToolNameLocalizationKey => "RECTANGLE_TOOL";
    public override bool IsErasable => false;

    public VectorRectangleToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
    }

    public override Type[]? SupportedLayerTypes { get; } = [];
    public override LocalizedString Tooltip => new LocalizedString("RECTANGLE_TOOL_TOOLTIP", Shortcut);

    public override string DefaultIcon => PixiPerfectIcons.Square;

    public override Type LayerTypeToCreateOnEmptyUse { get; } = typeof(VectorLayerNode);
    public string? DefaultNewLayerName { get; } = new LocalizedString("NEW_RECTANGLE_LAYER_NAME");

    public override void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
    {
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

    public override void OnSelected(bool restoring)
    {
        if (restoring) return;

        var document = ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument;
        var layer = document.SelectedStructureMember;
        if (layer is IVectorLayerHandler vectorLayer &&
            vectorLayer.GetShapeData(document.AnimationDataViewModel.ActiveFrameTime) is IReadOnlyRectangleData)
        {
            ShapeCorners corners = vectorLayer.TransformationCorners;
            ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument.TransformViewModel.ShowTransform(
                DocumentTransformMode.Scale_Rotate_Shear_NoPerspective, false, corners, false);
        }

        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseVectorRectangleTool();
    }

    protected override void OnSelectedLayersChanged(IStructureMemberHandler[] layers)
    {
        OnDeselecting(false);
        OnSelected(false);
    }
}
