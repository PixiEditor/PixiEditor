using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using PixiEditor.Numerics;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class RasterEllipseToolExecutor : ShapeToolExecutor<IRasterEllipseToolHandler>
{
    private void DrawEllipseOrCircle(VecI curPos, double rotationRad, bool firstDraw)
    {
        RectI rect;
        if (firstDraw)
            rect = new RectI(curPos, VecI.Zero);
        else if (toolViewModel!.DrawCircle)
            rect = GetSquaredCoordinates(startPos, curPos);
        else
            rect = RectI.FromTwoPixels(startPos, curPos);

        lastRect = rect;
        lastRadians = rotationRad;

        internals!.ActionAccumulator.AddActions(new DrawRasterEllipse_Action(memberGuid, rect, rotationRad, StrokeColor, FillColor, StrokeWidth, drawOnMask, document!.AnimationHandler.ActiveFrameBindable));
    }

    public override ExecutorType Type => ExecutorType.ToolLinked;
    protected override DocumentTransformMode TransformMode => DocumentTransformMode.Scale_Rotate_NoShear_NoPerspective;
    protected override void DrawShape(VecI currentPos, double rotationRad, bool firstDraw) => DrawEllipseOrCircle(currentPos, rotationRad, firstDraw);
    protected override IAction SettingsChangedAction()
    {
        return new DrawRasterEllipse_Action(memberGuid, lastRect, lastRadians, StrokeColor, FillColor, StrokeWidth, drawOnMask, document!.AnimationHandler.ActiveFrameBindable);
    }

    protected override IAction TransformMovedAction(ShapeData data, ShapeCorners corners)
    {
        RectI rect = (RectI)RectD.FromCenterAndSize(data.Center, data.Size);
        double radians = corners.RectRotation;
        
        lastRect = rect;
        lastRadians = radians;
        
        return new DrawRasterEllipse_Action(memberGuid, lastRect, lastRadians, StrokeColor,
            FillColor, StrokeWidth, drawOnMask, document!.AnimationHandler.ActiveFrameBindable);
    }

    protected override IAction EndDrawAction() => new EndDrawRasterEllipse_Action();
}
