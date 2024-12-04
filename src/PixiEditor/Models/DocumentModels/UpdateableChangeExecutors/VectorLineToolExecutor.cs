using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Models.Handlers.Tools;
using Drawie.Numerics;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class VectorLineToolExecutor : LineExecutor<IVectorLineToolHandler> 
{
    private VecD startPoint;
    private VecD endPoint;

    protected override bool AlignToPixels => false;

    protected override bool InitShapeData(IReadOnlyLineData? data)
    {
        if (data is null)
            return false;

        startPoint = data.Start;
        endPoint = data.End;

        return true;
    }

    protected override IAction DrawLine(VecD pos)
    {
        LineVectorData data = new LineVectorData(startDrawingPos, pos)
        {
            StrokeColor = StrokeColor,
            StrokeWidth = (float)StrokeWidth,
        };
        
        startPoint = startDrawingPos;
        endPoint = pos;

        return new SetShapeGeometry_Action(memberId, data);
    }

    protected override IAction TransformOverlayMoved(VecD start, VecD end)
    {
        LineVectorData data = new LineVectorData(start, end)
        {
            StrokeColor = StrokeColor,
            StrokeWidth = (float)StrokeWidth,
        };
        
        startPoint = start;
        endPoint = end;

        return new SetShapeGeometry_Action(memberId, data);
    }

    protected override IAction SettingsChange()
    {
        return TransformOverlayMoved(startPoint, endPoint);
    }

    protected override IAction EndDraw()
    {
        return new EndSetShapeGeometry_Action();
    }
}
