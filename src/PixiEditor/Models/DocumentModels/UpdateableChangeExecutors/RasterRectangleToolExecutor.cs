using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using PixiEditor.Numerics;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class RasterRectangleToolExecutor : ShapeToolExecutor<IRectangleToolHandler>
{
    private ShapeData lastData;
    public override ExecutorType Type => ExecutorType.ToolLinked;
    private void DrawRectangle(VecI curPos, double rotationRad, bool firstDraw)
    {
        RectI rect;
        if (firstDraw)
            rect = new RectI(curPos, VecI.Zero);
        else if (toolViewModel!.DrawSquare)
            rect = GetSquaredCoordinates(startPos, curPos);
        else
            rect = RectI.FromTwoPixels(startPos, curPos);
        lastRect = rect;
        lastRadians = rotationRad;
        
        lastData = new ShapeData(rect.Center, rect.Size, rotationRad, StrokeWidth, StrokeColor, FillColor);

        internals!.ActionAccumulator.AddActions(new DrawRectangle_Action(memberGuid, lastData, drawOnMask, document!.AnimationHandler.ActiveFrameBindable));
    }

    protected override void DrawShape(VecI currentPos, double rotationRad, bool first) => DrawRectangle(currentPos, rotationRad, first);
    protected override IAction SettingsChangedAction()
    {
        lastData = new ShapeData(lastData.Center, lastData.Size, lastRadians, StrokeWidth, StrokeColor, FillColor);
        return new DrawRectangle_Action(memberGuid, lastData, drawOnMask, document!.AnimationHandler.ActiveFrameBindable);   
    }

    protected override IAction TransformMovedAction(ShapeData data, ShapeCorners corners)
    {
        lastData = data;
        
        lastRadians = corners.RectRotation;
        
        return new DrawRectangle_Action(memberGuid, data, drawOnMask, document!.AnimationHandler.ActiveFrameBindable);
    }

    protected override IAction EndDrawAction() => new EndDrawRectangle_Action();
}
