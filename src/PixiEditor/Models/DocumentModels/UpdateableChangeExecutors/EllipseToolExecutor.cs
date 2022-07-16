using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class EllipseToolExecutor : ShapeToolExecutor<EllipseToolViewModel>
{
    private void DrawEllipseOrCircle(VecI curPos)
    {
        RectI rect = RectI.FromTwoPoints(startPos, curPos);
        if (rect.Width == 0)
            rect.Width = 1;
        if (rect.Height == 0)
            rect.Height = 1;

        if (toolViewModel!.DrawCircle)
            rect.Width = rect.Height = Math.Min(rect.Width, rect.Height);
        lastRect = rect;

        helpers!.ActionAccumulator.AddActions(new DrawEllipse_Action(memberGuid, rect, strokeColor, fillColor, strokeWidth, drawOnMask));
    }

    protected override void DrawShape(VecI currentPos) => DrawEllipseOrCircle(currentPos);

    protected override IAction TransformMovedAction(ShapeData data, ShapeCorners corners) =>
        new DrawEllipse_Action(memberGuid, (RectI)RectD.FromCenterAndSize(data.Center, data.Size), strokeColor,
            fillColor, strokeWidth, drawOnMask);

    protected override IAction EndDrawAction() => new EndDrawEllipse_Action();
}
