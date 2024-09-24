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

internal abstract class ComplexShapeToolExecutor<T> : UpdateableChangeExecutor where T : IShapeToolHandler
{
    protected int StrokeWidth => toolbar.ToolSize;

    protected Color FillColor =>
        toolbar.Fill ? toolbar.FillColor.ToColor() : DrawingApi.Core.ColorsImpl.Colors.Transparent;

    protected Color StrokeColor => toolbar.StrokeColor.ToColor();
    protected Guid memberGuid;
    protected bool drawOnMask;

    protected bool transforming = false;
    protected T? toolViewModel;
    protected VecI startPos;
    protected VecI unsnappedStartPos;
    protected RectI lastRect;
    protected double lastRadians;

    private bool noMovement = true;
    private IBasicShapeToolbar toolbar;
    private IColorsHandler? colorsVM;
    private bool previewMode = false;

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
            unsnappedStartPos = startPos;
            OnColorChanged(colorsVM.PrimaryColor, true);
            DrawShape(startPos, 0, true);
        }
        else
        {
            if (member is IVectorLayerHandler)
            {
                var node = (VectorLayerNode)internals.Tracker.Document.FindMember(member.Id);
                if (!InitShapeData(node.ShapeData))
                {
                    document.TransformHandler.HideTransform();
                    previewMode = true;
                    return ExecutionState.Success;
                }

                transforming = true;
                toolbar.StrokeColor = node.ShapeData.StrokeColor.ToColor();
                toolbar.FillColor = node.ShapeData.FillColor.ToColor();
                toolbar.ToolSize = node.ShapeData.StrokeWidth;
                toolbar.Fill = node.ShapeData.FillColor != Colors.Transparent;
            }
            else
            {
                previewMode = true;
            }
        }

        document.SnappingHandler.Remove(member.Id.ToString());

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
        Span<VecI> positions = stackalloc VecI[]
        {
            (VecI)(curPos.ProjectOntoLine(startPos, startPos + new VecD(1, 1)) -
                   new VecD(0.25).Multiply((curPos - startPos).Signs())).Round(),
            (VecI)(curPos.ProjectOntoLine(startPos, startPos + new VecD(1, -1)) -
                   new VecD(0.25).Multiply((curPos - startPos).Signs())).Round(),
            (VecI)(curPos.ProjectOntoLine(startPos, startPos + new VecD(1, 0)) -
                   new VecD(0.25).Multiply((curPos - startPos).Signs())).Round(),
            (VecI)(curPos.ProjectOntoLine(startPos, startPos + new VecD(0, 1)) -
                   new VecD(0.25).Multiply((curPos - startPos).Signs())).Round()
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
        if (!transforming)
            return;
        
        internals!.ActionAccumulator.AddFinishedActions(EndDrawAction());
        document!.TransformHandler.HideTransform();

        AddToSnapController();
        HighlightSnapAxis(null, null);

        colorsVM.AddSwatch(StrokeColor.ToPaletteColor());
        colorsVM.AddSwatch(FillColor.ToPaletteColor());

        previewMode = true;
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
        if (previewMode)
        {
            VecD mouseSnap = document.SnappingHandler.SnappingController.GetSnapPoint(pos, out string snapXAxis, out string snapYAxis);
            HighlightSnapAxis(snapXAxis, snapYAxis);
            
            if (!string.IsNullOrEmpty(snapXAxis) || !string.IsNullOrEmpty(snapYAxis))
            {
                document.SnappingHandler.SnappingController.HighlightedPoint = mouseSnap;
            }
            else
            {
                document.SnappingHandler.SnappingController.HighlightedPoint = null;
            }
        }

        if (transforming || previewMode)
            return;

        startPos = Snap(unsnappedStartPos, pos);
        var snapped = Snap(pos, startPos);
        
        noMovement = false;

        pos = snapped;

        DrawShape(pos, lastRadians, false);
    }

    private VecI Snap(VecI pos, VecD adjustPos)
    {
        VecI snapped =
            (VecI)document.SnappingHandler.SnappingController.GetSnapPoint(pos, out string snapXAxis,
                out string snapYAxis);

        HighlightSnapAxis(snapXAxis, snapYAxis);

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

    public override void OnLeftMouseButtonUp()
    {
        HighlightSnapAxis(null, null);

        if (transforming)
            return;

        if (noMovement)
        {
            internals!.ActionAccumulator.AddFinishedActions(EndDrawAction());
            AddToSnapController();

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

        AddToSnapController();
        HighlightSnapAxis(null, null);
    }

    private void AddToSnapController()
    {
        var member = document!.StructureHelper.Find(memberGuid);
        document.SnappingHandler.AddFromBounds(member.Id.ToString(), () => member.TightBounds ?? RectD.Empty);
    }
}
