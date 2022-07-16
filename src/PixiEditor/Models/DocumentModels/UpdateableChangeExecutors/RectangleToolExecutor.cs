using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class RectangleToolExecutor : ShapeToolExecutor<RectangleToolViewModel>
{
    private void DrawRectangle(VecI curPos)
    {
        RectI rect = RectI.FromTwoPoints(startPos, curPos);
        if (rect.Width == 0)
            rect.Width = 1;
        if (rect.Height == 0)
            rect.Height = 1;
        
        lastRect = rect;

        helpers!.ActionAccumulator.AddActions(new DrawRectangle_Action(memberGuid, new ShapeData(rect.Center, rect.Size, 0, strokeWidth, strokeColor, fillColor), drawOnMask));
    }

    protected override void DrawShape(VecI currentPos) => DrawRectangle(currentPos);

    protected override IAction TransformMovedAction(ShapeData data) => new DrawRectangle_Action(memberGuid, data, drawOnMask);

    protected override IAction EndDrawAction() => new EndDrawRectangle_Action();
}
