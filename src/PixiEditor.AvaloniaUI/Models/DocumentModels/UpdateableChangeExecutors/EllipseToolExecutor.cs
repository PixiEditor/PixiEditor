using ChunkyImageLib.DataHolders;
using PixiEditor.AvaloniaUI.Models.Handlers.Tools;
using PixiEditor.AvaloniaUI.Models.Tools;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class EllipseToolExecutor : ShapeToolExecutor<IEllipseToolHandler>
{
    private void DrawEllipseOrCircle(VecI curPos, bool firstDraw)
    {
        RectI rect;
        if (firstDraw)
            rect = new RectI(curPos, VecI.Zero);
        else if (toolViewModel!.DrawCircle)
            rect = GetSquaredCoordinates(startPos, curPos);
        else
            rect = RectI.FromTwoPixels(startPos, curPos);

        lastRect = rect;

        internals!.ActionAccumulator.AddActions(new DrawEllipse_Action(memberGuid, rect, strokeColor, fillColor, strokeWidth, drawOnMask));
    }

    public override ExecutorType Type => ExecutorType.ToolLinked;
    protected override DocumentTransformMode TransformMode => DocumentTransformMode.Scale_NoRotate_NoShear_NoPerspective;
    protected override void DrawShape(VecI currentPos, bool firstDraw) => DrawEllipseOrCircle(currentPos, firstDraw);

    protected override IAction TransformMovedAction(ShapeData data, ShapeCorners corners) =>
        new DrawEllipse_Action(memberGuid, (RectI)RectD.FromCenterAndSize(data.Center, data.Size), strokeColor,
            fillColor, strokeWidth, drawOnMask);

    protected override IAction EndDrawAction() => new EndDrawEllipse_Action();
}
