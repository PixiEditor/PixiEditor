using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using PixiEditor.Numerics;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal abstract class LineExecutor<T> : UpdateableChangeExecutor where T : ILineToolHandler
{
    public override ExecutorType Type => ExecutorType.ToolLinked;

    protected VecI startPos;
    protected Color StrokeColor => colorsVM!.PrimaryColor;
    protected int StrokeWidth => toolViewModel!.ToolSize;
    protected Guid memberGuid;
    protected bool drawOnMask;

    protected VecI curPos;
    private bool started = false;
    private bool transforming = false;
    private T? toolViewModel;
    private IColorsHandler? colorsVM;

    public override ExecutionState Start()
    {
        colorsVM = GetHandler<IColorsHandler>();
        toolViewModel = GetHandler<T>();
        IStructureMemberHandler? member = document?.SelectedStructureMember;
        if (colorsVM is null || toolViewModel is null || member is null)
            return ExecutionState.Error;

        drawOnMask = member is not ILayerHandler layer || layer.ShouldDrawOnMask;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not ILayerHandler)
            return ExecutionState.Error;

        startPos = controller!.LastPixelPosition;
        memberGuid = member.Id;

        return ExecutionState.Success;
    }

    protected abstract IAction DrawLine(VecI pos);
    protected abstract IAction TransformOverlayMoved(VecD start, VecD end);
    protected abstract IAction SettingsChange();
    protected abstract IAction EndDraw();

    public override void OnPixelPositionChange(VecI pos)
    {
        if (transforming)
            return;
        started = true;

        if (toolViewModel!.Snap)
            pos = ShapeToolExecutor<IShapeToolHandler>.Get45IncrementedPosition(startPos, pos);
        curPos = pos;
        var drawLineAction = DrawLine(pos);
        internals!.ActionAccumulator.AddActions(drawLineAction);
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


        var moveOverlayAction = TransformOverlayMoved(start, end);
        internals!.ActionAccumulator.AddActions(moveOverlayAction);

        startPos = (VecI)start;
        curPos = (VecI)end;
    }

    public override void OnColorChanged(Color color, bool primary)
    {
        if (!primary)
            return;

        var colorChangedAction = SettingsChange();
        internals!.ActionAccumulator.AddActions(colorChangedAction);
    }

    public override void OnSelectedObjectNudged(VecI distance)
    {
        if (!transforming)
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
        var endDrawAction = EndDraw();
        internals!.ActionAccumulator.AddFinishedActions(endDrawAction);
        onEnded!(this);

        colorsVM.AddSwatch(new PaletteColor(StrokeColor.R, StrokeColor.G, StrokeColor.B));
    }

    public override void ForceStop()
    {
        if (transforming)
            document!.LineToolOverlayHandler.Hide();

        var endDrawAction = EndDraw();
        internals!.ActionAccumulator.AddFinishedActions(endDrawAction);
    }
}
