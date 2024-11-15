using ChunkyImageLib.DataHolders;
using PixiEditor.Helpers.Extensions;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

#nullable enable

internal abstract class ComplexShapeToolExecutor<T> : SimpleShapeToolExecutor where T : IShapeToolHandler
{
    protected int StrokeWidth => toolbar.ToolSize;

    protected Color FillColor =>
        toolbar.Fill ? toolbar.FillColor.ToColor() : Colors.Transparent;

    protected Color StrokeColor => toolbar.StrokeColor.ToColor();
    protected bool drawOnMask;

    protected T? toolViewModel;
    protected RectI lastRect;
    protected double lastRadians;

    private bool noMovement = true;
    protected IBasicShapeToolbar toolbar;
    private IColorsHandler? colorsVM;

    public override bool CanUndo => document.TransformHandler.HasUndo;
    public override bool CanRedo => document.TransformHandler.HasRedo;

    public override ExecutionState Start()
    {
        if (base.Start() == ExecutionState.Error)
            return ExecutionState.Error;

        colorsVM = GetHandler<IColorsHandler>();
        toolViewModel = GetHandler<T>();
        toolbar = (IBasicShapeToolbar?)toolViewModel?.Toolbar;
        IStructureMemberHandler? member = document?.SelectedStructureMember;
        if (colorsVM is null || toolbar is null || member is null)
            return ExecutionState.Error;
        drawOnMask = member is not ILayerHandler layer || layer.ShouldDrawOnMask;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not ILayerHandler)
            return ExecutionState.Error;

        if (ActiveMode == ShapeToolMode.Drawing)
        {
            if (toolbar.SyncWithPrimaryColor)
            {
                toolbar.FillColor = colorsVM.PrimaryColor.ToColor();
                toolbar.StrokeColor = colorsVM.PrimaryColor.ToColor();
            }

            return ExecutionState.Success;
        }

        if (member is IVectorLayerHandler)
        {
            var node = (VectorLayerNode)internals.Tracker.Document.FindMember(member.Id);

            if (node == null)
            {
                return ExecutionState.Error;
            }

            if (node.ShapeData == null || !InitShapeData(node.ShapeData))
            {
                ActiveMode = ShapeToolMode.Preview;
                return ExecutionState.Success;
            }

            toolbar.StrokeColor = node.ShapeData.StrokeColor.ToColor();
            toolbar.FillColor = node.ShapeData.FillColor.ToColor();
            toolbar.ToolSize = node.ShapeData.StrokeWidth;
            toolbar.Fill = node.ShapeData.FillColor != Colors.Transparent;
            ActiveMode = ShapeToolMode.Transform;
        }
        else
        {
            ActiveMode = ShapeToolMode.Preview;
        }

        return ExecutionState.Success;
    }

    protected abstract void DrawShape(VecI currentPos, double rotationRad, bool firstDraw);
    protected abstract IAction SettingsChangedAction();
    protected abstract IAction TransformMovedAction(ShapeData data, ShapeCorners corners);
    protected virtual bool InitShapeData(ShapeVectorData data) { return true; }
    protected abstract IAction EndDrawAction();
    protected virtual DocumentTransformMode TransformMode => DocumentTransformMode.Scale_Rotate_NoShear_NoPerspective;

    public static VecI Get45IncrementedPosition(VecD startPos, VecD curPos)
    {
        Span<VecI> positions =
        [
            (VecI)(curPos.ProjectOntoLine(startPos, startPos + new VecD(1, 1)) -
                   new VecD(0.25).Multiply((curPos - startPos).Signs())).Round(),
            (VecI)(curPos.ProjectOntoLine(startPos, startPos + new VecD(1, -1)) -
                   new VecD(0.25).Multiply((curPos - startPos).Signs())).Round(),
            (VecI)(curPos.ProjectOntoLine(startPos, startPos + new VecD(1, 0)) -
                   new VecD(0.25).Multiply((curPos - startPos).Signs())).Round(),
            (VecI)(curPos.ProjectOntoLine(startPos, startPos + new VecD(0, 1)) -
                   new VecD(0.25).Multiply((curPos - startPos).Signs())).Round()
        ];

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
        VecI pos1 = (VecI)(((VecD)curPos).ProjectOntoLine(startPos, startPos + new VecD(1, 1)) -
                           new VecD(0.25).Multiply((curPos - startPos).Signs())).Round();
        VecI pos2 = (VecI)(((VecD)curPos).ProjectOntoLine(startPos, startPos + new VecD(1, -1)) -
                           new VecD(0.25).Multiply((curPos - startPos).Signs())).Round();
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
        if (ActiveMode != ShapeToolMode.Transform)
            return;

        var rect = RectD.FromCenterAndSize(corners.RectCenter, corners.RectSize);
        ShapeData shapeData = new ShapeData(rect.Center, rect.Size, corners.RectRotation, StrokeWidth, StrokeColor,
            FillColor) { AntiAliasing = toolbar.AntiAliasing };
        IAction drawAction = TransformMovedAction(shapeData, corners);

        internals!.ActionAccumulator.AddActions(drawAction);
    }

    public override void OnTransformApplied()
    {
        if (ActiveMode != ShapeToolMode.Transform)
            return;

        internals!.ActionAccumulator.AddFinishedActions(EndDrawAction());
        document!.TransformHandler.HideTransform();

        colorsVM.AddSwatch(StrokeColor.ToPaletteColor());
        colorsVM.AddSwatch(FillColor.ToPaletteColor());

        base.OnTransformApplied();
    }

    public override void OnColorChanged(Color color, bool primary)
    {
        if (primary && toolbar.SyncWithPrimaryColor && ActiveMode == ShapeToolMode.Transform)
        {
            toolbar.StrokeColor = color.ToColor();
            toolbar.FillColor = color.ToColor();
        }
    }

    public override void OnSelectedObjectNudged(VecI distance)
    {
        if (ActiveMode != ShapeToolMode.Transform)
            return;
        document!.TransformHandler.Nudge(distance);
    }

    public override void OnMidChangeUndo()
    {
        if (ActiveMode != ShapeToolMode.Transform)
            return;
        document!.TransformHandler.Undo();
    }

    public override void OnMidChangeRedo()
    {
        if (ActiveMode != ShapeToolMode.Transform)
            return;
        document!.TransformHandler.Redo();
    }

    protected override void PrecisePositionChangeDrawingMode(VecD pos)
    {
        var snapped = Snap(pos, startDrawingPos, true);

        noMovement = false;

        DrawShape((VecI)snapped.Floor(), lastRadians, false);
    }

    protected VecD Snap(VecD pos, VecD adjustPos, bool highlight = false)
    {
        VecD snapped =
            document.SnappingHandler.SnappingController.GetSnapPoint(pos, out string snapXAxis,
                out string snapYAxis);

        if (highlight)
        {
            HighlightSnapAxis(snapXAxis, snapYAxis);
        }

        if (snapped != VecI.Zero)
        {
            if (adjustPos.X < pos.X)
            {
                snapped -= new VecI(1, 0);
            }

            if (adjustPos.Y < pos.Y)
            {
                snapped -= new VecI(0, 1);
            }
        }

        return snapped;
    }

    private void HighlightSnapAxis(string snapXAxis, string snapYAxis)
    {
        document.SnappingHandler.SnappingController.HighlightedXAxis = snapXAxis;
        document.SnappingHandler.SnappingController.HighlightedYAxis = snapYAxis;
        document.SnappingHandler.SnappingController.HighlightedPoint = null;
    }

    public override void OnSettingsChanged(string name, object value)
    {
        internals!.ActionAccumulator.AddActions(SettingsChangedAction());
    }

    public override void OnLeftMouseButtonUp(VecD argsPositionOnCanvas)
    {
        if (ActiveMode != ShapeToolMode.Transform)
        {
            if (noMovement)
            {
                internals!.ActionAccumulator.AddFinishedActions(EndDrawAction());
                AddMemberToSnapping();

                base.OnLeftMouseButtonUp(argsPositionOnCanvas);
                onEnded?.Invoke(this);
                return;
            }
        }

        base.OnLeftMouseButtonUp(argsPositionOnCanvas);
    }

    protected override void StartMode(ShapeToolMode mode)
    {
        base.StartMode(mode);
        if (mode == ShapeToolMode.Transform)
        {
            document!.TransformHandler.ShowTransform(TransformMode, false, new ShapeCorners((RectD)lastRect), true);
        }
    }

    public override void ForceStop()
    {
        base.ForceStop();
        internals!.ActionAccumulator.AddFinishedActions(EndDrawAction());
    }
}
