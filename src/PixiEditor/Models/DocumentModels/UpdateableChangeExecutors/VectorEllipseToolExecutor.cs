using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using PixiEditor.Numerics;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class VectorEllipseToolExecutor : ShapeToolExecutor<IVectorEllipseToolHandler>
{
    public override ExecutorType Type => ExecutorType.ToolLinked;
    protected override DocumentTransformMode TransformMode => DocumentTransformMode.Scale_Rotate_Shear_NoPerspective;

    protected override void DrawShape(VecI curPos, double rotationRad, bool firstDraw)
    {
        RectI rect;
        if (firstDraw)
            rect = new RectI(curPos, VecI.Zero);
        else if (toolViewModel!.DrawCircle)
            rect = GetSquaredCoordinates(startPos, curPos);
        else
            rect = RectI.FromTwoPixels(startPos, curPos);
        
        EllipseVectorData data = new EllipseVectorData(rect.Center, rect.Size / 2f) 
        {
            RotationRadians = rotationRad, 
            StrokeColor = StrokeColor, 
            FillColor = FillColor, 
            StrokeWidth = StrokeWidth
        };
        
        lastRect = rect;
        lastRadians = rotationRad;
        
        internals!.ActionAccumulator.AddActions(new SetShapeGeometry_Action(memberGuid, data));
    }

    protected override IAction SettingsChangedAction()
    {
        return new SetShapeGeometry_Action(memberGuid, new EllipseVectorData(lastRect.Center, lastRect.Size / 2f) 
        {
            RotationRadians = lastRadians, 
            StrokeColor = StrokeColor, 
            FillColor = FillColor, 
            StrokeWidth = StrokeWidth
        });
    }

    protected override IAction TransformMovedAction(ShapeData data, ShapeCorners corners)
    {
        RectI rect = (RectI)RectD.FromCenterAndSize(data.Center, data.Size);
        double radians = corners.RectRotation;
        
        EllipseVectorData ellipseData = new EllipseVectorData(rect.Center, rect.Size / 2f) 
        {
            RotationRadians = radians, 
            StrokeColor = StrokeColor, 
            FillColor = FillColor, 
            StrokeWidth = StrokeWidth
        };
        
        lastRect = rect;
        lastRadians = radians;
        
        return new SetShapeGeometry_Action(memberGuid, ellipseData);
    }

    protected override IAction EndDrawAction()
    {
        return new EndSetShapeGeometry_Action();
    }
}
