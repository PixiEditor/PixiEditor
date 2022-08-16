using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class EllipseToolExecutor : ShapeToolExecutor<EllipseToolViewModel>
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
    protected override DocumentTransformMode TransformMode => DocumentTransformMode.NoRotation;
    protected override void DrawShape(VecI currentPos, bool firstDraw) => DrawEllipseOrCircle(currentPos, firstDraw);

    protected override IAction TransformMovedAction(ShapeData data, ShapeCorners corners) =>
        new DrawEllipse_Action(memberGuid, (RectI)RectD.FromCenterAndSize(data.Center, data.Size), strokeColor,
            fillColor, strokeWidth, drawOnMask);

    protected override IAction EndDrawAction() => new EndDrawEllipse_Action();
}
