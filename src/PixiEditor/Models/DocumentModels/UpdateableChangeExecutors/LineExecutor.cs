using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using PixiEditor.Numerics;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal abstract class LineExecutor<T> : SimpleShapeToolExecutor where T : ILineToolHandler
{
    public override ExecutorType Type => ExecutorType.ToolLinked;

    protected Color StrokeColor => toolbar!.StrokeColor.ToColor();
    protected int StrokeWidth => toolViewModel!.ToolSize;
    protected bool drawOnMask;

    protected VecD curPos;
    private bool startedDrawing = false;
    private T? toolViewModel;
    private IColorsHandler? colorsVM;
    private ILineToolbar? toolbar;

    public override ExecutionState Start()
    {
        if (base.Start() == ExecutionState.Error)
            return ExecutionState.Error;

        colorsVM = GetHandler<IColorsHandler>();
        toolViewModel = GetHandler<T>();
        IStructureMemberHandler? member = document?.SelectedStructureMember;
        toolbar = (ILineToolbar?)toolViewModel?.Toolbar;
        if (colorsVM is null || toolViewModel is null || member is null)
            return ExecutionState.Error;

        drawOnMask = member is not ILayerHandler layer || layer.ShouldDrawOnMask;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not ILayerHandler)
            return ExecutionState.Error;

        if (ActiveMode == ShapeToolMode.Drawing)
        {
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

            toolbar.StrokeColor = data.StrokeColor.ToColor();

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

        VecD snapped =
            document!.SnappingHandler.SnappingController.GetSnapPoint(pos, out string snapX, out string snapY);

        if (toolViewModel!.Snap)
        {
            snapped = ComplexShapeToolExecutor<IShapeToolHandler>.Get45IncrementedPosition(startDrawingPos, pos);
        }

        HighlightSnapping(snapX, snapY);

        curPos = snapped;

        var drawLineAction = DrawLine(curPos);
        internals!.ActionAccumulator.AddActions(drawLineAction);
    }

    public override void OnLeftMouseButtonUp()
    {
        if (!startedDrawing)
        {
            onEnded!(this);
            return;
        }

        document!.LineToolOverlayHandler.Show(startDrawingPos, curPos, true);
        base.OnLeftMouseButtonUp();
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
        if (!primary)
            return;

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
}
