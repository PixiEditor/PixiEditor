using Avalonia.Media;
using ChunkyImageLib.DataHolders;
using PixiEditor.Helpers.Extensions;
using PixiEditor.ChangeableDocument.Actions;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changes.Vectors;
using PixiEditor.ViewModels.Document.TransformOverlays;
using PixiEditor.Views.Overlays.TransformOverlay;
using Color = Drawie.Backend.Core.ColorsImpl.Color;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

#nullable enable

internal abstract class DrawableShapeToolExecutor<T> : SimpleShapeToolExecutor where T : IShapeToolHandler
{
    protected double StrokeWidth => toolbar.ToolSize;

    protected Paintable FillPaintable =>
        toolbar.Fill ? toolbar.FillBrush?.ToPaintable() : Colors.Transparent;

    protected Paintable StrokePaintable => toolbar.StrokeBrush.ToPaintable();
    protected bool drawOnMask;

    protected T? toolViewModel;
    protected RectD lastRect;
    protected double lastRadians;

    protected virtual bool DeleteLayerOnNoDraw => false;
    protected virtual bool SelectLayerOnTap => false;
    protected virtual Predicate<ILayerHandler> CanSelectLayer => x => true;

    private ShapeCorners initialCorners;
    private bool noMovement = true;
    protected IFillableShapeToolbar toolbar;
    private IColorsHandler? colorsVM;
    private bool ignoreNextColorChange = false;

    private bool preventSettingsChange = false;

    protected abstract bool UseGlobalUndo { get; }
    protected abstract bool ShowApplyButton { get; }

    public override bool CanUndo => !UseGlobalUndo && document.TransformHandler.HasUndo;
    public override bool CanRedo => !UseGlobalUndo && document.TransformHandler.HasRedo;

    protected virtual bool ApplyEachSettingsChange => false;

    public override ExecutionState Start()
    {
        if (base.Start() == ExecutionState.Error)
            return ExecutionState.Error;

        colorsVM = GetHandler<IColorsHandler>();
        toolViewModel = GetHandler<T>();
        toolbar = (IFillableShapeToolbar?)toolViewModel?.Toolbar;
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
                toolbar.FillBrush = new SolidColorBrush(colorsVM.PrimaryColor.ToColor());
                toolbar.StrokeBrush = new SolidColorBrush(colorsVM.PrimaryColor.ToColor());
                ignoreNextColorChange = colorsVM.ColorsTempSwapped;
            }

            lastRect = new RectD(startDrawingPos, VecD.Zero);

            document!.TransformHandler.ShowTransform(TransformMode, false, new ShapeCorners(lastRect),
                false, UseGlobalUndo ? AddToUndo : null);
            document.TransformHandler.ShowHandles = false;
            document.TransformHandler.IsSizeBoxEnabled = true;
            document.TransformHandler.CanAlignToPixels = AlignToPixels;

            return ExecutionState.Success;
        }

        if (member is IVectorLayerHandler vectorLayerHandler)
        {
            var shapeData = vectorLayerHandler.GetShapeData(document.AnimationHandler.ActiveFrameTime);
            bool shapeIsValid = InitShapeData(shapeData);
            if (shapeData == null || !shapeIsValid)
            {
                ActiveMode = ShapeToolMode.Preview;
                return ExecutionState.Success;
            }

            toolbar.StrokeBrush = shapeData.Stroke.ToBrush();
            toolbar.FillBrush = shapeData.FillPaintable.ToBrush();
            toolbar.ToolSize = shapeData.StrokeWidth;
            toolbar.Fill = shapeData.FillPaintable.AnythingVisible;
            initialCorners = shapeData.TransformationCorners;

            ShapeCorners corners = vectorLayerHandler.TransformationCorners;
            document.TransformHandler.ShowTransform(
                TransformMode, false, corners, false, UseGlobalUndo ? AddToUndo : null);
            document.TransformHandler.CanAlignToPixels = false;

            ActiveMode = ShapeToolMode.Transform;
        }
        else
        {
            ActiveMode = ShapeToolMode.Preview;
        }

        return ExecutionState.Success;
    }

    protected abstract void DrawShape(VecD currentPos, double rotationRad, bool firstDraw);
    protected abstract IAction SettingsChangedAction(string name, object value);
    protected abstract IAction TransformMovedAction(ShapeData data, ShapeCorners corners);
    protected virtual bool InitShapeData(IReadOnlyShapeVectorData data) { return false; }
    protected abstract bool CanEditShape(IStructureMemberHandler layer);
    protected abstract IAction EndDrawAction();
    protected virtual DocumentTransformMode TransformMode => DocumentTransformMode.Scale_Rotate_NoShear_NoPerspective;


    public static VecI GetSquaredPosition(VecI startPos, VecI curPos)
    {
        VecI pos1 = (VecI)(((VecD)curPos).ProjectOntoLine(startPos, startPos + new VecD(1, 1)) -
                           new VecD(0.25).Multiply((curPos - startPos).Signs())).Round();
        VecI pos2 = (VecI)(((VecD)curPos).ProjectOntoLine(startPos, startPos + new VecD(1, -1)) -
                           new VecD(0.25).Multiply((curPos - startPos).Signs())).Round();
        if ((pos1 - curPos).LengthSquared > (pos2 - curPos).LengthSquared)
            return pos2;
        return pos1;
    }

    public static VecD GetSquaredPosition(VecD startPos, VecD curPos)
    {
        VecD pos1 = curPos.ProjectOntoLine(startPos, startPos + new VecD(1, 1)) -
                    new VecD(0.25).Multiply((curPos - (VecI)startPos).Signs());
        VecD pos2 = curPos.ProjectOntoLine(startPos, startPos + new VecD(1, -1)) -
                    new VecD(0.25).Multiply((curPos - (VecI)startPos).Signs());
        if ((pos1 - curPos).LengthSquared > (pos2 - curPos).LengthSquared)
            return pos2;
        return pos1;
    }

    public override void OnTransformChanged(ShapeCorners corners)
    {
        if (ActiveMode != ShapeToolMode.Transform)
            return;

        var shapeData = ShapeDataFromCorners(corners);
        IAction drawAction = TransformMovedAction(shapeData, corners);

        internals!.ActionAccumulator.AddActions(drawAction);
    }

    private ShapeData ShapeDataFromCorners(ShapeCorners corners)
    {
        var rect = RectD.FromCenterAndSize(corners.RectCenter, corners.RectSize);
        double cornerRadius = 0;
        if (toolViewModel is ICornerRadiusTool cornerRadiusTool)
        {
            cornerRadius = cornerRadiusTool.CornerRadius;
        }

        ShapeData shapeData = new ShapeData(rect.Center, rect.Size, cornerRadius, corners.RectRotation,
            (float)StrokeWidth,
            StrokePaintable,
            FillPaintable) { AntiAliasing = toolbar.AntiAliasing };
        return shapeData;
    }

    public override void OnTransformApplied()
    {
        if (ActiveMode != ShapeToolMode.Transform)
            return;

        internals!.ActionAccumulator.AddFinishedActions(EndDrawAction());
        document!.TransformHandler.HideTransform();

        // TODO: Add other paintables support
        if (StrokePaintable is ColorPaintable strokeColor)
        {
            colorsVM.AddSwatch(strokeColor.Color.ToPaletteColor());
        }

        if (FillPaintable is ColorPaintable fillColor)
        {
            colorsVM.AddSwatch(fillColor.Color.ToPaletteColor());
        }

        base.OnTransformApplied();
    }

    public override void OnColorChanged(Color color, bool primary)
    {
        if (!primary || !toolbar.SyncWithPrimaryColor || ActiveMode == ShapeToolMode.Preview || ignoreNextColorChange)
        {
            if (primary)
            {
                ignoreNextColorChange = false;
            }

            return;
        }

        ignoreNextColorChange = ActiveMode == ShapeToolMode.Drawing;

        preventSettingsChange = true;
        toolbar.StrokeBrush = new SolidColorBrush(color.ToColor());
        toolbar.FillBrush = new SolidColorBrush(color.ToColor());
        preventSettingsChange = false;

        var layer = document.StructureHelper.Find(memberId);
        if (layer is null)
            return;

        if (CanEditShape(layer))
        {
            internals!.ActionAccumulator.AddFinishedActions(
                EndDrawAction(),
                SettingsChangedAction("FillAndStroke", color),
                EndDrawAction());
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

        document.TransformHandler.Undo();
    }

    public override void OnMidChangeRedo()
    {
        if (ActiveMode != ShapeToolMode.Transform)
            return;

        document.TransformHandler.Redo();
    }

    protected override void PrecisePositionChangeDrawingMode(VecD pos)
    {
        VecD adjustedPos = AlignToPixels ? (VecI)pos.Floor() : pos;

        VecD startPos = startDrawingPos;

        VecD snapped;
        if (toolViewModel.DrawEven)
        {
            adjustedPos = AlignToPixels
                ? GetSquaredPosition((VecI)startDrawingPos, (VecI)adjustedPos)
                : GetSquaredPosition(startPos, adjustedPos);
            VecD dir = (adjustedPos - startDrawingPos).Normalize();
            snapped = Snap(adjustedPos, startDrawingPos, dir, true);
        }
        else
        {
            snapped = Snap(adjustedPos, startDrawingPos, true);
        }

        noMovement = false;

        if (toolViewModel.DrawFromCenter)
        {
            VecD center = startDrawingPos;

            startDrawingPos = center + (center - snapped);
        }

        if (AlignToPixels)
        {
            DrawShape((VecI)snapped.Floor(), lastRadians, false);
        }
        else
        {
            DrawShape(snapped, lastRadians, false);
        }

        startDrawingPos = startPos;

        document!.TransformHandler.ShowTransform(TransformMode, false, new ShapeCorners((RectD)lastRect), false,
            UseGlobalUndo ? AddToUndo : null);
        document.TransformHandler.CanAlignToPixels = AlignToPixels;
        document!.TransformHandler.Corners = new ShapeCorners((RectD)lastRect);
    }

    protected VecD Snap(VecD pos, VecD adjustPos, bool highlight = false)
    {
        VecD snapped =
            document.SnappingHandler.SnappingController.GetSnapPoint(pos, out string snapXAxis,
                out string snapYAxis);

        if (highlight)
        {
            HighlightSnapAxis(snapXAxis, snapYAxis, string.IsNullOrEmpty(snapXAxis) && string.IsNullOrEmpty(snapYAxis) ? null : snapped);
        }

        if (AlignToPixels)
        {
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
        }

        return snapped;
    }

    protected VecD Snap(VecD pos, VecD adjustPos, VecD dir, bool highlight = false)
    {
        VecD snapped =
            document.SnappingHandler.SnappingController.GetSnapPoint(pos, dir, out string snapXAxis,
                out string snapYAxis);

        if (highlight)
        {
            HighlightSnapAxis(snapXAxis, snapYAxis, string.IsNullOrEmpty(snapXAxis) && string.IsNullOrEmpty(snapYAxis) ? null : snapped);
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

    private void HighlightSnapAxis(string snapXAxis, string snapYAxis, VecD? snapPoint)
    {
        document.SnappingHandler.SnappingController.HighlightedXAxis = snapXAxis;
        document.SnappingHandler.SnappingController.HighlightedYAxis = snapYAxis;
        document.SnappingHandler.SnappingController.HighlightedPoint = snapPoint;
    }

    public override void OnSettingsChanged(string name, object value)
    {
        if (preventSettingsChange) return;

        var layer = document.StructureHelper.Find(memberId);
        if (layer is null)
            return;

        if (CanEditShape(layer))
        {
            if (ApplyEachSettingsChange)
            {
                internals!.ActionAccumulator.AddFinishedActions(EndDrawAction(), SettingsChangedAction(name, value),
                    EndDrawAction());
            }
            else
            {
                internals!.ActionAccumulator.AddActions(SettingsChangedAction(name, value));
            }
        }
    }

    public override void OnLeftMouseButtonUp(VecD argsPositionOnCanvas)
    {
        if (ActiveMode != ShapeToolMode.Transform)
        {
            if (noMovement)
            {
                internals!.ActionAccumulator.AddFinishedActions(EndDrawAction());
                AddMembersToSnapping();

                base.OnLeftMouseButtonUp(argsPositionOnCanvas);
                onEnded?.Invoke(this);

                if (DeleteLayerOnNoDraw)
                {
                    if (lastRect.Size == VecD.Zero)
                    {
                        var member = document!.StructureHelper.Find(memberId);
                        if (member is not null)
                        {
                            internals.ActionAccumulator.AddActions(ActionSource.Automated,
                                new DeleteStructureMember_Action(memberId));
                            //internals.ActionAccumulator.AddFinishedActions();
                            document.TransformHandler.HideTransform();
                        }
                    }
                }

                if (SelectLayerOnTap)
                {
                    var layersUnderCursor = QueryLayers<ILayerHandler>(argsPositionOnCanvas);
                    var firstValidLayer = layersUnderCursor.FirstOrDefault(x => CanSelectLayer(x));
                    if (firstValidLayer != null)
                    {
                        document.Operations.SetSelectedMember(firstValidLayer.Id);
                    }
                }

                return;
            }
        }

        base.OnLeftMouseButtonUp(argsPositionOnCanvas);
    }

    protected override void StopMode(ShapeToolMode mode)
    {
        base.StopMode(mode);
        if (mode == ShapeToolMode.Drawing)
        {
            initialCorners = new ShapeCorners((RectD)lastRect);
        }
    }

    protected override void StartMode(ShapeToolMode mode)
    {
        base.StartMode(mode);
        if (mode == ShapeToolMode.Transform)
        {
            document.TransformHandler.HideTransform();
            document!.TransformHandler.ShowTransform(TransformMode, false, initialCorners, ShowApplyButton,
                UseGlobalUndo ? AddToUndo : null);
            document.TransformHandler.CanAlignToPixels = AlignToPixels;
        }
    }

    public override void ForceStop()
    {
        base.ForceStop();
        internals!.ActionAccumulator.AddFinishedActions(EndDrawAction());
    }

    protected override void StopTransformMode()
    {
        document!.TransformHandler.HideTransform();
    }

    private void AddToUndo(ShapeCorners corners)
    {
        if (UseGlobalUndo)
        {
            internals!.ActionAccumulator.AddFinishedActions(EndDrawAction(),
                TransformMovedAction(ShapeDataFromCorners(corners), corners), EndDrawAction());
        }
    }
}
