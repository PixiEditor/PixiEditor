using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using PixiEditor.Numerics;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal abstract class LineExecutor<T> : UpdateableChangeExecutor where T : ILineToolHandler
{
    public override ExecutorType Type => ExecutorType.ToolLinked;

    protected VecI startPos;
    protected Color StrokeColor => toolbar!.StrokeColor.ToColor();
    protected int StrokeWidth => toolViewModel!.ToolSize;
    protected Guid memberGuid;
    protected bool drawOnMask;

    protected VecI curPos;
    private bool started = false;
    private bool transforming = false;
    private T? toolViewModel;
    private IColorsHandler? colorsVM;
    private ILineToolbar? toolbar;

    public override ExecutionState Start()
    {
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

        memberGuid = member.Id;

        if (controller.LeftMousePressed || member is not IVectorLayerHandler)
        {
            startPos = controller!.LastPixelPosition;
            OnColorChanged(colorsVM.PrimaryColor, true);
        }
        else
        {
            transforming = true;
            var node = (VectorLayerNode)internals.Tracker.Document.FindMember(member.Id);
            IReadOnlyLineData data = node.ShapeData as IReadOnlyLineData;
            
            if(data is null)
            {
                document.TransformHandler.HideTransform();
                return ExecutionState.Error;
            }

            toolbar.StrokeColor = data.StrokeColor.ToColor();
            
            if (!InitShapeData(node.ShapeData as IReadOnlyLineData))
            {
                document.TransformHandler.HideTransform();
                return ExecutionState.Error;
            }
        }

        document.SnappingHandler.Remove(memberGuid.ToString());
        
        return ExecutionState.Success;
    }

    protected abstract bool InitShapeData(IReadOnlyLineData? data);
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

        document!.LineToolOverlayHandler.Show(startPos + new VecD(0.5), curPos + new VecD(0.5), true);
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

        toolbar!.StrokeColor = color.ToColor();
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
        AddMemberToSnapping();
        onEnded!(this);

        colorsVM.AddSwatch(new PaletteColor(StrokeColor.R, StrokeColor.G, StrokeColor.B));
    }

    public override void ForceStop()
    {
        if (transforming)
            document!.LineToolOverlayHandler.Hide();

        var endDrawAction = EndDraw();
        internals!.ActionAccumulator.AddFinishedActions(endDrawAction);
        AddMemberToSnapping();
    }
    
    private void AddMemberToSnapping()
    {
        var member = document.StructureHelper.Find(memberGuid);
        document!.SnappingHandler.AddFromBounds(memberGuid.ToString(), () => member!.TightBounds ?? RectD.Empty);
    }
}
