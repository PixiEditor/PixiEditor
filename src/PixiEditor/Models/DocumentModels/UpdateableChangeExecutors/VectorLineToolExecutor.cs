using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Models.Handlers.Tools;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changes.Vectors;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class VectorLineToolExecutor : LineExecutor<IVectorLineToolHandler>
{
    private VecD startPoint;
    private VecD endPoint;

    protected override bool AlignToPixels => false;

    protected override bool UseGlobalUndo => true;
    protected override bool ShowApplyButton => false;

    protected override bool InitShapeData(IReadOnlyLineData? data)
    {
        if (data is null)
            return false;

        startPoint = data.TransformedStart;
        endPoint = data.TransformedEnd;

        return true;
    }

    protected override IAction DrawLine(VecD pos)
    {
        LineVectorData data = ConstructLineData(startDrawingPos, pos);

        startPoint = startDrawingPos;
        endPoint = pos;

        return new SetShapeGeometry_Action(memberId, data, VectorShapeChangeType.GeometryData);
    }

    protected override IAction TransformOverlayMoved(VecD start, VecD end)
    {
        var data = ConstructLineData(start, end);

        startPoint = start;
        endPoint = end;

        return new SetShapeGeometry_Action(memberId, data, VectorShapeChangeType.GeometryData);
    }

    protected IAction SettingsChanged(string name, object value)
    {
        var data = ConstructLineData(startPoint, endPoint);

        VectorShapeChangeType changeType = name switch
        {
            nameof(IShapeToolbar.StrokeBrush) => VectorShapeChangeType.Stroke,
            nameof(IShapeToolbar.ToolSize) => VectorShapeChangeType.Stroke,
            nameof(IShapeToolbar.AntiAliasing) => VectorShapeChangeType.OtherVisuals,
            _ => VectorShapeChangeType.All
        };

        return new SetShapeGeometry_Action(memberId, data, changeType);
    }

    public override void OnLeftMouseButtonUp(VecD argsPositionOnCanvas)
    {
        base.OnLeftMouseButtonUp(argsPositionOnCanvas);

        if (!startedDrawing)
        {
            var member = document!.StructureHelper.Find(memberId);
            if (member is not null)
            {
                document.Operations.DeleteStructureMember(memberId);
                document.TransformHandler.HideTransform();
            }
        }

        var layersUnderCursor = QueryLayers<IVectorLayerHandler>(argsPositionOnCanvas);
        var firstValidLayer = layersUnderCursor.FirstOrDefault(x =>
            x.GetShapeData(document.AnimationHandler.ActiveFrameTime) is IReadOnlyLineData);
        if (firstValidLayer != null)
        {
            document.Operations.SetSelectedMember(firstValidLayer.Id);
        }
    }

    protected override IAction[] SettingsChange(string name, object value)
    {
        return [SettingsChanged(name, value), new EndSetShapeGeometry_Action()];
    }

    protected override IAction EndDraw()
    {
        return new EndSetShapeGeometry_Action();
    }

    public override bool IsFeatureEnabled<T>()
    {
        Type feature = typeof(T);
        if (feature == typeof(IMidChangeUndoableExecutor)) return false;

        return base.IsFeatureEnabled<T>();
    }
}
