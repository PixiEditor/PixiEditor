using ChunkyImageLib.DataHolders;
using PixiEditor.Helpers.Extensions;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using PixiEditor.Numerics;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

#nullable enable

internal abstract class ShapeToolExecutor<T> : UpdateableChangeExecutor where T : IShapeToolHandler
{
    protected int StrokeWidth => toolbar.ToolSize;
    protected Color FillColor => toolbar.Fill ? toolbar.FillColor.ToColor() : DrawingApi.Core.ColorsImpl.Colors.Transparent;
    protected Color StrokeColor => toolbar.StrokeColor.ToColor();
    protected Guid memberGuid;
    protected bool drawOnMask;

    protected bool transforming = false;
    protected T? toolViewModel;
    protected VecI startPos;
    protected RectI lastRect;
    protected double lastRadians;
    
    private bool noMovement = true;
    private IBasicShapeToolbar toolbar;
    private IColorsHandler? colorsVM;
    
    public override ExecutionState Start()
    {
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

        memberGuid = member.Id;
        
        if (controller.LeftMousePressed || member is not IVectorLayerHandler)
        {
            startPos = controller!.LastPixelPosition;
            OnColorChanged(colorsVM.PrimaryColor, true);
            DrawShape(startPos, 0, true);
        }
        else
        {
            transforming = true;
            if (member is IVectorLayerHandler)
            {
                var node = (VectorLayerNode)internals.Tracker.Document.FindMember(member.Id);
                if (!InitShapeData(node.ShapeData))
                {
                    document.TransformHandler.HideTransform();
                    return ExecutionState.Error;
                }
                
                toolbar.StrokeColor = node.ShapeData.StrokeColor.ToColor();
                toolbar.FillColor = node.ShapeData.FillColor.ToColor();
                toolbar.ToolSize = node.ShapeData.StrokeWidth;
                toolbar.Fill = node.ShapeData.FillColor != Colors.Transparent;
            }
        }
        
        
        return ExecutionState.Success;
    }

    protected abstract void DrawShape(VecI currentPos, double rotationRad, bool firstDraw);
    protected abstract IAction SettingsChangedAction();
    protected abstract IAction TransformMovedAction(ShapeData data, ShapeCorners corners);
    protected virtual bool InitShapeData(ShapeVectorData data) { return true; }
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
        ShapeData shapeData = new ShapeData(rect.Center, rect.Size, corners.RectRotation, StrokeWidth, StrokeColor,
            FillColor);
        IAction drawAction = TransformMovedAction(shapeData, corners);

        internals!.ActionAccumulator.AddActions(drawAction);
    }

    public override void OnTransformApplied()
    {
        internals!.ActionAccumulator.AddFinishedActions(EndDrawAction());
        document!.TransformHandler.HideTransform();
        onEnded?.Invoke(this);
        
        colorsVM.AddSwatch(StrokeColor.ToPaletteColor());
        colorsVM.AddSwatch(FillColor.ToPaletteColor());
    }
    
    public override void OnColorChanged(Color color, bool primary)
    {
        if (primary && toolbar.SyncWithPrimaryColor)
        {
            toolbar.StrokeColor = color.ToColor();
            toolbar.FillColor = color.ToColor();
        }
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
        DrawShape(pos, lastRadians, false);
    }

    public override void OnSettingsChanged(string name, object value)
    {
        internals!.ActionAccumulator.AddActions(SettingsChangedAction());
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
        
        
        document!.TransformHandler.HideTransform();
        document!.TransformHandler.ShowTransform(TransformMode, false, new ShapeCorners((RectD)lastRect), true);
        transforming = true;
    }

    public override void ForceStop()
    {
        if (transforming)
            document!.TransformHandler.HideTransform();
        internals!.ActionAccumulator.AddFinishedActions(EndDrawAction());
    }
}
