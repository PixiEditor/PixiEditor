﻿using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class RectangleToolExecutor : ShapeToolExecutor<RectangleToolViewModel>
{
    public override ExecutorType Type => ExecutorType.ToolLinked;
    private void DrawRectangle(VecI curPos, bool firstDraw)
    {
        RectI rect;
        if (firstDraw)
            rect = new RectI(curPos, VecI.Zero);
        else if (toolViewModel!.DrawSquare)
            rect = GetSquaredCoordinates(startPos, curPos);
        else
            rect = RectI.FromTwoPixels(startPos, curPos);
        lastRect = rect;

        internals!.ActionAccumulator.AddActions(new DrawRectangle_Action(memberGuid, new ShapeData(rect.Center, rect.Size, 0, strokeWidth, strokeColor, fillColor), drawOnMask));
    }

    protected override void DrawShape(VecI currentPos, bool first) => DrawRectangle(currentPos, first);

    protected override IAction TransformMovedAction(ShapeData data, ShapeCorners corners) => new DrawRectangle_Action(memberGuid, data, drawOnMask);

    protected override IAction EndDrawAction() => new EndDrawRectangle_Action();
}
