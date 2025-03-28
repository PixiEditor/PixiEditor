using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Models.Handlers.Tools;
using Drawie.Numerics;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;

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

        return new SetShapeGeometry_Action(memberId, data);
    }

    protected override IAction TransformOverlayMoved(VecD start, VecD end)
    {
        var data = ConstructLineData(start, end);
        
        startPoint = start;
        endPoint = end;

        return new SetShapeGeometry_Action(memberId, data);
    }

    protected override IAction[] SettingsChange()
    {
        return [TransformOverlayMoved(startPoint, endPoint), new EndSetShapeGeometry_Action()];
    }

    protected override IAction EndDraw()
    {
        return new EndSetShapeGeometry_Action();
    }

    public override bool IsFeatureEnabled<T>()
    {
        Type feature = typeof(T);
        if(feature == typeof(IMidChangeUndoableExecutor)) return false;
        
        return base.IsFeatureEnabled<T>();
    }
}
