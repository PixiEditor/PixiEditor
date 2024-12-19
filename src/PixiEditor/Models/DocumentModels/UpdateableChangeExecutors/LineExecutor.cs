using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers.InputDevice;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal abstract class LineExecutor<T> : SimpleShapeToolExecutor where T : ILineToolHandler
{
    public override ExecutorType Type => ExecutorType.ToolLinked;

    protected Color StrokeColor => toolbar!.StrokeColor.ToColor();
    protected double StrokeWidth => toolViewModel!.ToolSize;

    protected bool drawOnMask;

    protected VecD curPos;
    private bool startedDrawing = false;
    private T? toolViewModel;
    private IColorsHandler? colorsVM;
    protected IShapeToolbar? toolbar;
    private bool ignoreNextColorChange = false;
    private VecD lastStartPos;

    public override bool CanUndo => document.LineToolOverlayHandler.HasUndo;
    public override bool CanRedo => document.LineToolOverlayHandler.HasRedo;

    public override ExecutionState Start()
    {
        if (base.Start() == ExecutionState.Error)
            return ExecutionState.Error;

        colorsVM = GetHandler<IColorsHandler>();
        toolViewModel = GetHandler<T>();
        IStructureMemberHandler? member = document?.SelectedStructureMember;
        toolbar = (IShapeToolbar)toolViewModel?.Toolbar;
        if (colorsVM is null || toolViewModel is null || member is null)
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
                toolbar.StrokeColor = colorsVM.PrimaryColor.ToColor();
                ignoreNextColorChange = colorsVM.ColorsTempSwapped;
            }

            document.LineToolOverlayHandler.Hide();
            document.LineToolOverlayHandler.Show(startDrawingPos, startDrawingPos, false);
            document.LineToolOverlayHandler.ShowHandles = false;
            document.LineToolOverlayHandler.IsSizeBoxEnabled = true;

            return ExecutionState.Success;
        }

        if (member is IVectorLayerHandler)
        {
            var node = (VectorLayerNode)internals.Tracker.Document.FindMember(member.Id);

            if (node.ShapeData is not IReadOnlyLineData data)
            {
                ActiveMode = ShapeToolMode.Preview;
                return ExecutionState.Success;
            }

            if (!InitShapeData(data))
            {
                ActiveMode = ShapeToolMode.Preview;
                return ExecutionState.Success;
            }

            ActiveMode = ShapeToolMode.Transform;
        }
        else
        {
            ActiveMode = ShapeToolMode.Preview;
        }

        return ExecutionState.Success;
    }

    protected abstract bool InitShapeData(IReadOnlyLineData? data);
    protected abstract IAction DrawLine(VecD pos);
    protected abstract IAction TransformOverlayMoved(VecD start, VecD end);
    protected abstract IAction SettingsChange();
    protected abstract IAction EndDraw();

    protected override void PrecisePositionChangeDrawingMode(VecD pos)
    {
        startedDrawing = true;

        VecD endPos = pos;
        VecD snapped = endPos;
        string snapX = "";
        string snapY = "";

        VecD startPos = startDrawingPos;

        if (toolViewModel!.Snap)
        {
            if (AlignToPixels)
            {
                endPos = GeometryHelper.Get45IncrementedPositionAligned(startDrawingPos, pos);
            }
            else
            {
                endPos = GeometryHelper.Get45IncrementedPosition(startDrawingPos, pos);
            }

            VecD directionConstraint = endPos - startDrawingPos;
            snapped =
                document!.SnappingHandler.SnappingController.GetSnapPoint(endPos, directionConstraint, out snapX,
                    out snapY);
        }
        else
        {
            snapped = document!.SnappingHandler.SnappingController.GetSnapPoint(endPos, out snapX, out snapY);
        }

        if (toolViewModel.DrawFromCenter)
        {
            VecD center = startDrawingPos;
            startDrawingPos = center + (center - snapped);
        }

        HighlightSnapping(snapX, snapY);
        document!.LineToolOverlayHandler.LineEnd = snapped;

        curPos = snapped;

        var drawLineAction = DrawLine(curPos);
        internals!.ActionAccumulator.AddActions(drawLineAction);

        lastStartPos = startDrawingPos;
        startDrawingPos = startPos;
    }

    public override void OnLeftMouseButtonUp(VecD argsPositionOnCanvas)
    {
        if (!startedDrawing)
        {
            base.OnLeftMouseButtonUp(argsPositionOnCanvas);
            onEnded!(this);
            return;
        }

        document!.LineToolOverlayHandler.Hide();
        document!.LineToolOverlayHandler.Show(lastStartPos, curPos, true);
        base.OnLeftMouseButtonUp(argsPositionOnCanvas);
    }

    public override void OnLineOverlayMoved(VecD start, VecD end)
    {
        if (ActiveMode != ShapeToolMode.Transform)
            return;

        var moveOverlayAction = TransformOverlayMoved(start, end);
        internals!.ActionAccumulator.AddActions(moveOverlayAction);

        startDrawingPos = (VecI)start;
        curPos = (VecI)end;
    }

    public override void OnColorChanged(Color color, bool primary)
    {
        if (!primary || !toolbar!.SyncWithPrimaryColor || ActiveMode == ShapeToolMode.Preview || ignoreNextColorChange)
        {
            if (primary)
            {
                ignoreNextColorChange = false;
            }

            return;
        }

        ignoreNextColorChange = ActiveMode == ShapeToolMode.Drawing;
        toolbar!.StrokeColor = color.ToColor();
        var colorChangedAction = SettingsChange();
        internals!.ActionAccumulator.AddActions(colorChangedAction);
    }

    public override void OnSelectedObjectNudged(VecI distance)
    {
        if (ActiveMode != ShapeToolMode.Transform)
            return;

        document!.LineToolOverlayHandler.Nudge(distance);
    }

    public override void OnSettingsChanged(string name, object value)
    {
        var colorChangedAction = SettingsChange();
        internals!.ActionAccumulator.AddActions(colorChangedAction);
    }

    public override void OnMidChangeUndo()
    {
        if (ActiveMode != ShapeToolMode.Transform)
            return;

        document!.LineToolOverlayHandler.Undo();
    }

    public override void OnMidChangeRedo()
    {
        if (ActiveMode != ShapeToolMode.Transform)
            return;

        document!.LineToolOverlayHandler.Redo();
    }

    public override void OnTransformApplied()
    {
        base.OnTransformApplied();
        var endDrawAction = EndDraw();
        internals!.ActionAccumulator.AddFinishedActions(endDrawAction);

        colorsVM.AddSwatch(new PaletteColor(StrokeColor.R, StrokeColor.G, StrokeColor.B));
    }

    public override void ForceStop()
    {
        base.ForceStop();
        var endDrawAction = EndDraw();
        internals!.ActionAccumulator.AddFinishedActions(endDrawAction);
    }

    protected override void StopTransformMode()
    {
        document!.LineToolOverlayHandler.Hide();
    }
}
