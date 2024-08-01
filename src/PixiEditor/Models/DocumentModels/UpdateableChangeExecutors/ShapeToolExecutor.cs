using ChunkyImageLib.DataHolders;
using PixiEditor.Helpers.Extensions;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using PixiEditor.Numerics;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

#nullable enable

internal abstract class ShapeToolExecutor<T> : UpdateableChangeExecutor where T : IShapeToolHandler
{
    protected int strokeWidth;
    protected Color fillColor;
    protected Color strokeColor;
    protected Guid memberGuid;
    protected bool drawOnMask;

    protected bool transforming = false;
    protected T? toolViewModel;
    protected VecI startPos;
    protected RectI lastRect;

    private bool noMovement = true;

    public override ExecutionState Start()
    {
        IColorsHandler? colorsVM = GetHandler<IColorsHandler>();
        toolViewModel = GetHandler<T>();
        IBasicShapeToolbar? toolbar = (IBasicShapeToolbar?)toolViewModel?.Toolbar;
        IStructureMemberHandler? member = document?.SelectedStructureMember;
        if (colorsVM is null || toolbar is null || member is null)
            return ExecutionState.Error;
        drawOnMask = member is ILayerHandler layer ? layer.ShouldDrawOnMask : true;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not ILayerHandler)
            return ExecutionState.Error;

        fillColor = toolbar.Fill ? toolbar.FillColor.ToColor() : DrawingApi.Core.ColorsImpl.Colors.Transparent;
        startPos = controller!.LastPixelPosition;
        strokeColor = colorsVM.PrimaryColor;
        strokeWidth = toolbar.ToolSize;
        memberGuid = member.Id;

        colorsVM.AddSwatch(new PaletteColor(strokeColor.R, strokeColor.G, strokeColor.B));
        DrawShape(startPos, true);
        return ExecutionState.Success;
    }

    protected abstract void DrawShape(VecI currentPos, bool firstDraw);
    protected abstract IAction TransformMovedAction(ShapeData data, ShapeCorners corners);
    protected abstract IAction EndDrawAction();
    protected virtual DocumentTransformMode TransformMode => DocumentTransformMode.Scale_Rotate_NoShear_NoPerspective;

    public static VecI Get45IncrementedPosition(VecI startPos, VecI curPos)
    {
        Span<VecI> positions = stackalloc VecI[]
        {
            (VecI)(((VecD)curPos).ProjectOntoLine(startPos, startPos + new VecD(1, 1)) - new VecD(0.25).Multiply((curPos - startPos).Signs())).Round(),
            (VecI)(((VecD)curPos).ProjectOntoLine(startPos, startPos + new VecD(1, -1)) - new VecD(0.25).Multiply((curPos - startPos).Signs())).Round(),
            (VecI)(((VecD)curPos).ProjectOntoLine(startPos, startPos + new VecD(1, 0)) - new VecD(0.25).Multiply((curPos - startPos).Signs())).Round(),
            (VecI)(((VecD)curPos).ProjectOntoLine(startPos, startPos + new VecD(0, 1)) - new VecD(0.25).Multiply((curPos - startPos).Signs())).Round()
        };
        VecI max = positions[0];
        double maxLength = double.MaxValue;
        foreach (var pos in positions)
        {
            double length = (pos - curPos).LengthSquared;
            if (length < maxLength)
            {
                maxLength = length;
                max = pos;
            }
        }
        return max;
    }

    public static VecI GetSquaredPosition(VecI startPos, VecI curPos)
    {
        VecI pos1 = (VecI)(((VecD)curPos).ProjectOntoLine(startPos, startPos + new VecD(1, 1)) - new VecD(0.25).Multiply((curPos - startPos).Signs())).Round();
        VecI pos2 = (VecI)(((VecD)curPos).ProjectOntoLine(startPos, startPos + new VecD(1, -1)) - new VecD(0.25).Multiply((curPos - startPos).Signs())).Round();
        if ((pos1 - curPos).LengthSquared > (pos2 - curPos).LengthSquared)
            return (VecI)pos2;
        return (VecI)pos1;
    }

    public static RectI GetSquaredCoordinates(VecI startPos, VecI curPos)
    {
        VecI pos = GetSquaredPosition(startPos, curPos);
        return RectI.FromTwoPixels(startPos, pos);
    }

    public override void OnTransformMoved(ShapeCorners corners)
    {
        if (!transforming)
            return;

        var rect = RectD.FromCenterAndSize(corners.RectCenter, corners.RectSize);
        ShapeData shapeData = new ShapeData(rect.Center, rect.Size, corners.RectRotation, strokeWidth, strokeColor,
            fillColor);
        IAction drawAction = TransformMovedAction(shapeData, corners);

        internals!.ActionAccumulator.AddActions(drawAction);
    }

    public override void OnTransformApplied()
    {
        internals!.ActionAccumulator.AddFinishedActions(EndDrawAction());
        document!.TransformHandler.HideTransform();
        onEnded?.Invoke(this);
    }

    public override void OnSelectedObjectNudged(VecI distance)
    {
        if (!transforming)
            return;
        document!.TransformHandler.Nudge(distance);
    }

    public override void OnMidChangeUndo()
    {
        if (!transforming)
            return;
        document!.TransformHandler.Undo();
    }

    public override void OnMidChangeRedo()
    {
        if (!transforming)
            return;
        document!.TransformHandler.Redo();
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        if (transforming)
            return;
        noMovement = false;
        DrawShape(pos, false);
    }

    public override void OnLeftMouseButtonUp()
    {
        if (transforming)
            return;
        if (noMovement)
        {
            internals!.ActionAccumulator.AddFinishedActions(EndDrawAction());
            onEnded?.Invoke(this);
            return;
        }
        transforming = true;
        document!.TransformHandler.ShowTransform(TransformMode, false, new ShapeCorners((RectD)lastRect), true);
    }

    public override void ForceStop()
    {
        if (transforming)
            document!.TransformHandler.HideTransform();
        internals!.ActionAccumulator.AddFinishedActions(EndDrawAction());
    }
}
