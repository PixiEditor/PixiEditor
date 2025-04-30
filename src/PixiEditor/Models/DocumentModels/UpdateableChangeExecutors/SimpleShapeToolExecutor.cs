using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Utils;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.Models.Controllers.InputDevice;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

/// <summary>
/// This class is responsible for handling the execution of a simple shape tool.
///  This executor handles: Shape tool state management and snapping
///  Drawing a shape can be either on raster layer or vector.
/// 
///  - Preview mode: a state when the tool is selected and editing of current shape is disabled or impossible. During this state,
///      snapping overlays are shown under mouse position, axes and snap point.
///  - Drawing mode: a state when the user clicked on the canvas and is dragging the mouse to draw a shape.
///        During this state, snapping axes are highlighted.
///  - Transform mode: a state when the user is transforming existing shape.
///     During this state, snapping axes are highlighted.
///
///     Possible state transitions:
///         - Preview -> Drawing (when user clicks on the canvas)
///         - Drawing -> Transform (when user releases the mouse after drawing)
///         - Transform -> Preview (when user applies the transform)
///         - Transform -> Drawing (when user clicks outside of shape transform bounds)
/// </summary>
internal abstract class SimpleShapeToolExecutor : UpdateableChangeExecutor,
    ITransformableExecutor, IMidChangeUndoableExecutor, IDelayedColorSwapFeature
{
    private ShapeToolMode activeMode;

    protected ShapeToolMode ActiveMode
    {
        get => activeMode;
        set
        {
            if (activeMode == value)
                return;

            StopMode(activeMode);
            activeMode = value;
            StartMode(activeMode);
        }
    }

    protected virtual bool AlignToPixels { get; } = true;

    protected Guid memberId;
    protected VecD startDrawingPos;

    private IDisposable restoreSnapping;

    public override bool BlocksOtherActions => ActiveMode == ShapeToolMode.Drawing;

    public override ExecutionState Start()
    {
        IStructureMemberHandler? member = document?.SelectedStructureMember;

        if (member is null)
            return ExecutionState.Error;

        memberId = member.Id;

        if (controller.LeftMousePressed)
        {
            ActiveMode = ShapeToolMode.Drawing;
        }
        else
        {
            ActiveMode = ShapeToolMode.Preview;
        }

        if (controller.LeftMousePressed)
        {
            restoreSnapping?.Dispose();
            restoreSnapping = DisableSelfSnapping(memberId, document);
        }

        return ExecutionState.Success;
    }

    protected virtual void StartMode(ShapeToolMode mode)
    {
        switch (mode)
        {
            case ShapeToolMode.Preview:
                break;
            case ShapeToolMode.Drawing:
                StartDrawingMode();
                break;
            case ShapeToolMode.Transform:
                break;
        }
    }

    private void StartDrawingMode()
    {
        var snapped = SnapAndHighlight(controller.LastPrecisePosition);
        if (AlignToPixels)
        {
            startDrawingPos = (VecI)snapped;
        }
        else
        {
            startDrawingPos = snapped;
        }
    }

    protected virtual void StopMode(ShapeToolMode mode)
    {
        switch (mode)
        {
            case ShapeToolMode.Preview:
                break;
            case ShapeToolMode.Drawing:
                break;
            case ShapeToolMode.Transform:
                StopTransformMode();
                break;
        }
    }

    protected abstract void StopTransformMode();

    public override void OnLeftMouseButtonDown(MouseOnCanvasEventArgs args)
    {
        if (ActiveMode == ShapeToolMode.Preview)
        {
            ActiveMode = ShapeToolMode.Drawing;
        }

        restoreSnapping?.Dispose();
        restoreSnapping = DisableSelfSnapping(memberId, document);
    }

    public override void OnPrecisePositionChange(VecD pos)
    {
        if (ActiveMode == ShapeToolMode.Preview)
        {
            PrecisePositionChangePreviewMode(pos);
        }
        else if (ActiveMode == ShapeToolMode.Drawing)
        {
            PrecisePositionChangeDrawingMode(pos);
        }
        else if (ActiveMode == ShapeToolMode.Transform)
        {
            PrecisePositionChangeTransformMode(pos);
        }
    }

    public override void OnLeftMouseButtonUp(VecD argsPositionOnCanvas)
    {
        HighlightSnapping(null, null);
        ActiveMode = ShapeToolMode.Transform;
    }

    public bool IsTransforming => ActiveMode == ShapeToolMode.Transform;

    public virtual void OnTransformChanged(ShapeCorners corners)
    {
    }

    public virtual void OnTransformApplied()
    {
        ActiveMode = ShapeToolMode.Preview;
        AddMembersToSnapping();
        HighlightSnapping(null, null);
    }

    public virtual void OnLineOverlayMoved(VecD start, VecD end)
    {
    }

    public virtual void OnSelectedObjectNudged(VecI distance)
    {
    }

    public override void ForceStop()
    {
        StopMode(activeMode);
        AddMembersToSnapping();
        HighlightSnapping(null, null);
    }

    protected void HighlightSnapping(string? snapX, string? snapY)
    {
        document!.SnappingHandler.SnappingController.HighlightedXAxis = snapX;
        document!.SnappingHandler.SnappingController.HighlightedYAxis = snapY;
        document.SnappingHandler.SnappingController.HighlightedPoint = null;
    }

    protected void AddMembersToSnapping()
    {
        restoreSnapping?.Dispose();
    }

    protected VecD SnapAndHighlight(VecD pos)
    {
        VecD snapped =
            document.SnappingHandler.SnappingController.GetSnapPoint(pos, out string snapX, out string snapY);
        HighlightSnapping(snapX, snapY);
        return snapped;
    }

    protected virtual void PrecisePositionChangePreviewMode(VecD pos)
    {
        VecD mouseSnap =
            document.SnappingHandler.SnappingController.GetSnapPoint(pos, out string snapXAxis,
                out string snapYAxis);
        HighlightSnapping(snapXAxis, snapYAxis);

        if (!string.IsNullOrEmpty(snapXAxis) || !string.IsNullOrEmpty(snapYAxis))
        {
            document.SnappingHandler.SnappingController.HighlightedPoint = mouseSnap;
        }
        else
        {
            document.SnappingHandler.SnappingController.HighlightedPoint = null;
        }
    }

    public static IDisposable DisableSelfSnapping(Guid memberId, IDocument document)
    {
        List<Guid> disabledSnappingMembers = new();
        disabledSnappingMembers.Add(memberId);
        document.SnappingHandler.Remove(memberId.ToString());

        Guid child = memberId;

        var parents = document.StructureHelper.GetParents(child);

        foreach (var parent in parents)
        {
            disabledSnappingMembers.Add(parent.Id);
            document.SnappingHandler.Remove(parent.Id.ToString());
        }

        return Disposable.Create(() =>
        {
            foreach (var id in disabledSnappingMembers)
            {
                var member = document.StructureHelper.Find(id);
                if (member != null && member.IsVisibleBindable)
                {
                    document.SnappingHandler.AddFromBounds(id.ToString(), () => member?.TightBounds ?? RectD.Empty);
                }
            }
        });
    }

    protected virtual void PrecisePositionChangeDrawingMode(VecD pos) { }
    protected virtual void PrecisePositionChangeTransformMode(VecD pos) { }
    public abstract void OnMidChangeUndo();
    public abstract void OnMidChangeRedo();
    public abstract bool CanUndo { get; }
    public abstract bool CanRedo { get; }

    public virtual bool IsFeatureEnabled<T>()
    {
        Type t = typeof(T);
        if (t == typeof(ITransformableExecutor))
        {
            return IsTransforming;
        }

        if (t == typeof(IMidChangeUndoableExecutor))
        {
            return ActiveMode == ShapeToolMode.Transform;
        }

        if (t == typeof(IDelayedColorSwapFeature))
        {
            return true;
        }

        return false;
    }
}

enum ShapeToolMode
{
    Preview,
    Drawing,
    Transform
}
