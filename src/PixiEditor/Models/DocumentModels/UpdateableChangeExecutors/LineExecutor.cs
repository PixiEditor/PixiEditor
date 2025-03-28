using Avalonia.Media;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.ViewModels.Document.TransformOverlays;
using Color = Drawie.Backend.Core.ColorsImpl.Color;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal abstract class LineExecutor<T> : SimpleShapeToolExecutor where T : ILineToolHandler
{
    public override ExecutorType Type => ExecutorType.ToolLinked;

    protected Paintable StrokePaintable => toolbar!.StrokeBrush.ToPaintable();
    protected double StrokeWidth => toolViewModel!.ToolSize;
    protected abstract bool UseGlobalUndo { get; }
    protected abstract bool ShowApplyButton { get; }

    protected bool drawOnMask;

    protected VecD curPos;
    protected bool startedDrawing = false;
    private T? toolViewModel;
    private IColorsHandler? colorsVM;
    protected IShapeToolbar? toolbar;
    private bool ignoreNextColorChange = false;
    private VecD lastStartPos;

    private UndoStack<LineVectorData>? localUndoStack;

    public override bool CanUndo => !UseGlobalUndo && localUndoStack is { UndoCount: > 0 };
    public override bool CanRedo => !UseGlobalUndo && localUndoStack is { RedoCount: > 0 };

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
        
        localUndoStack = new UndoStack<LineVectorData>();

        if (ActiveMode == ShapeToolMode.Drawing)
        {
            if (toolbar.SyncWithPrimaryColor)
            {
                toolbar.StrokeBrush = new SolidColorBrush(colorsVM.PrimaryColor.ToColor());
                ignoreNextColorChange = colorsVM.ColorsTempSwapped;
            }

            document.LineToolOverlayHandler.Hide();
            document.LineToolOverlayHandler.Show(startDrawingPos, startDrawingPos, false, AddToUndo);
            document.LineToolOverlayHandler.ShowHandles = false;
            document.LineToolOverlayHandler.IsSizeBoxEnabled = true;

            return ExecutionState.Success;
        }

        if (member is IVectorLayerHandler)
        {
            var node = (VectorLayerNode)internals.Tracker.Document.FindMember(member.Id);
            
            if(node is null)
                return ExecutionState.Error;

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

            document.LineToolOverlayHandler.Show(data.TransformedStart, data.TransformedEnd, false, AddToUndo);
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
    protected abstract IAction[] SettingsChange(string name, object value);
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
            internals.ActionAccumulator.AddFinishedActions(EndDraw());
            AddMembersToSnapping();
            
            base.OnLeftMouseButtonUp(argsPositionOnCanvas);
            ActiveMode = ShapeToolMode.Preview;
            onEnded!(this);
            return;
        }

        AddToUndo((lastStartPos, curPos));
        document!.LineToolOverlayHandler.Hide();
        document!.LineToolOverlayHandler.Show(lastStartPos, curPos, ShowApplyButton, AddToUndo);
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
        toolbar!.StrokeBrush = new SolidColorBrush(color.ToColor());
        var colorChangedAction = SettingsChange(nameof(IShapeToolbar.StrokeBrush), color);
        internals!.ActionAccumulator.AddActions(colorChangedAction);
    }

    public override void OnSelectedObjectNudged(VecI distance)
    {
        if (ActiveMode != ShapeToolMode.Transform)
            return;

        document!.LineToolOverlayHandler.Nudge(distance);
        AddToUndo((document.LineToolOverlayHandler.LineStart, document.LineToolOverlayHandler.LineEnd));
    }

    public override void OnSettingsChanged(string name, object value)
    {
        var colorChangedActions = SettingsChange(name, value);
        if (ActiveMode == ShapeToolMode.Transform)
        {
            internals!.ActionAccumulator.AddFinishedActions(colorChangedActions);
        }
    }

    public override void OnMidChangeUndo()
    {
        if (ActiveMode != ShapeToolMode.Transform || localUndoStack == null)
            return;

        var undone = localUndoStack?.Undo();
        if (undone is not null)
        {
            ApplyState(undone);
        }
    }

    public override void OnMidChangeRedo()
    {
        if (ActiveMode != ShapeToolMode.Transform || localUndoStack == null)
            return;

        var redone = localUndoStack?.Redo();
        if (redone is not null)
        {
            ApplyState(redone);
        }
    }

    public override void OnTransformApplied()
    {
        base.OnTransformApplied();
        var endDrawAction = EndDraw();
        internals!.ActionAccumulator.AddFinishedActions(endDrawAction);

        if (StrokePaintable is ColorPaintable colorPaintable)
        {
            colorsVM.AddSwatch(new PaletteColor(colorPaintable.Color.R, colorPaintable.Color.G, colorPaintable.Color.B));
        }
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

    protected override void StartMode(ShapeToolMode mode)
    {
        base.StartMode(mode);
        if (mode == ShapeToolMode.Transform)
        {
            document!.LineToolOverlayHandler.Hide();
            document!.LineToolOverlayHandler.Show(lastStartPos, curPos, ShowApplyButton, AddToUndo);
        }
    }

    private void AddToUndo((VecD, VecD) newPos)
    {
        if (UseGlobalUndo)
        {
            internals!.ActionAccumulator.AddFinishedActions(EndDraw(), TransformOverlayMoved(newPos.Item1, newPos.Item2), EndDraw());
        }
        else
        {
            localUndoStack!.AddState(ConstructLineData(newPos.Item1, newPos.Item2));
        }
    }

    protected LineVectorData ConstructLineData(VecD start, VecD end)
    {
        return new LineVectorData(start, end) { StrokeWidth = (float)StrokeWidth, Stroke = StrokePaintable };
    }
    
    private void ApplyState(LineVectorData data)
    {
        toolbar!.StrokeBrush = data.Stroke.ToBrush();
        toolbar!.ToolSize = data.StrokeWidth;
        
        document!.LineToolOverlayHandler.Show(data.Start, data.End, ShowApplyButton, AddToUndo);
    }
}
