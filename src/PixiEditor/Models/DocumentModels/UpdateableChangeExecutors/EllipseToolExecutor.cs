using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class EllipseToolExecutor : ShapeToolExecutor<EllipseToolViewModel>
{
    private void DrawEllipseOrCircle(VecI curPos)
    {
        RectI rect;
        if (toolViewModel!.DrawCircle)
            rect = GetSquaredCoordinates(startPos, curPos);
        else
            rect = RectI.FromTwoPixels(startPos, curPos);

        lastRect = rect;

        internals!.ActionAccumulator.AddActions(new DrawEllipse_Action(memberGuid, rect, strokeColor, fillColor, strokeWidth, drawOnMask));
    }

    protected override DocumentTransformMode TransformMode => DocumentTransformMode.NoRotation;
    protected override void DrawShape(VecI currentPos) => DrawEllipseOrCircle(currentPos);

    protected override IAction TransformMovedAction(ShapeData data, ShapeCorners corners) =>
        new DrawEllipse_Action(memberGuid, (RectI)RectD.FromCenterAndSize(data.Center, data.Size), strokeColor,
            fillColor, strokeWidth, drawOnMask);

    protected override IAction EndDrawAction() => new EndDrawEllipse_Action();
}
