﻿using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class LineToolExecutor : ShapeToolExecutor<LineToolViewModel>
{
    public override ExecutorType Type => ExecutorType.ToolLinked;
    public override ExecutionState Start()
    {
        ColorsViewModel? colorsVM = ViewModelMain.Current?.ColorsSubViewModel;
        toolViewModel = ViewModelMain.Current?.ToolsSubViewModel.GetTool<LineToolViewModel>();
        BasicToolbar? toolbar = (BasicToolbar?)toolViewModel?.Toolbar;
        StructureMemberViewModel? member = document?.SelectedStructureMember;
        if (colorsVM is null || toolbar is null || member is null)
            return ExecutionState.Error;
        drawOnMask = member is LayerViewModel layer ? layer.ShouldDrawOnMask : true;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not LayerViewModel)
            return ExecutionState.Error;

        startPos = controller!.LastPixelPosition;
        strokeColor = colorsVM.PrimaryColor;
        strokeWidth = toolbar.ToolSize;
        memberGuid = member.GuidValue;

        colorsVM.AddSwatch(strokeColor);
        DrawShape(startPos, true);
        return ExecutionState.Success;
    }

    private void DrawLine(VecI curPos, bool firstDraw)
    {
        RectI rect = firstDraw ? new RectI(curPos, VecI.Zero) : RectI.FromTwoPixels(startPos, curPos);
        if (rect.Width == 0)
            rect.Width = 1;
        if (rect.Height == 0)
            rect.Height = 1;

        lastRect = rect;

        internals!.ActionAccumulator.AddActions(new DrawLine_Action(memberGuid, startPos, curPos, strokeWidth, strokeColor, StrokeCap.Butt, drawOnMask));
    }

    protected override void DrawShape(VecI currentPos, bool firstDraw) => DrawLine(currentPos, firstDraw);

    protected override IAction TransformMovedAction(ShapeData data, ShapeCorners corners)
    {
        return new DrawLine_Action(memberGuid, (VecI)corners.TopLeft, (VecI)corners.BottomRight.Ceiling(), strokeWidth, strokeColor, StrokeCap.Butt,
            drawOnMask);
    }

    protected override IAction EndDrawAction() => new EndDrawLine_Action();
}