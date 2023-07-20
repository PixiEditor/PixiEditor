using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Models.Containers;
using PixiEditor.Models.Containers.Tools;
using PixiEditor.Models.Enums;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class LineToolExecutor : UpdateableChangeExecutor
{
    public override ExecutorType Type => ExecutorType.ToolLinked;

    private VecI startPos;
    private Color strokeColor;
    private int strokeWidth;
    private Guid memberGuid;
    private bool drawOnMask;

    private VecI curPos;
    private bool started = false;
    private bool transforming = false;
    private ILineToolHandler? toolViewModel;

    public override ExecutionState Start()
    {
        IColorsHandler? colorsVM = GetHandler<IColorsHandler>();
        toolViewModel = GetHandler<ILineToolHandler>();
        IStructureMemberHandler? member = document?.SelectedStructureMember;
        if (colorsVM is null || toolViewModel is null || member is null)
            return ExecutionState.Error;

        drawOnMask = member is not ILayerHandler layer || layer.ShouldDrawOnMask;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not ILayerHandler)
            return ExecutionState.Error;

        startPos = controller!.LastPixelPosition;
        strokeColor = colorsVM.PrimaryColor;
        strokeWidth = toolViewModel.ToolSize;
        memberGuid = member.GuidValue;

        colorsVM.AddSwatch(new PaletteColor(strokeColor.R, strokeColor.G, strokeColor.B));

        return ExecutionState.Success;
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        if (transforming)
            return;
        started = true;

        if (toolViewModel!.Snap)
            pos = ShapeToolExecutor<IShapeToolHandler>.Get45IncrementedPosition(startPos, pos);
        curPos = pos;
        internals!.ActionAccumulator.AddActions(new DrawLine_Action(memberGuid, startPos, pos, strokeWidth, strokeColor, StrokeCap.Butt, drawOnMask));
    }

    public override void OnLeftMouseButtonUp()
    {
        if (!started)
        {
            onEnded!(this);
            return;
        }

        document!.LineToolOverlayHandler.Show(startPos + new VecD(0.5), curPos + new VecD(0.5));
        transforming = true;
    }

    public override void OnLineOverlayMoved(VecD start, VecD end)
    {
        if (!transforming)
            return;
        internals!.ActionAccumulator.AddActions(new DrawLine_Action(memberGuid, (VecI)start, (VecI)end, strokeWidth, strokeColor, StrokeCap.Butt, drawOnMask));
    }

    public override void OnSelectedObjectNudged(VecI distance)
    {
        if (!transforming)
            return;
        document!.LineToolOverlayHandler.Nudge(distance);
    }

    public override void OnMidChangeUndo()
    {
        if (!transforming)
            return;
        document!.LineToolOverlayHandler.Undo();
    }

    public override void OnMidChangeRedo()
    {
        if (!transforming)
            return;
        document!.LineToolOverlayHandler.Redo();
    }

    public override void OnTransformApplied()
    {
        if (!transforming)
            return;

        document!.LineToolOverlayHandler.Hide();
        internals!.ActionAccumulator.AddFinishedActions(new EndDrawLine_Action());
        onEnded!(this);
    }

    public override void ForceStop()
    {
        if (transforming)
            document!.LineToolOverlayHandler.Hide();

        internals!.ActionAccumulator.AddFinishedActions(new EndDrawLine_Action());
    }
}
