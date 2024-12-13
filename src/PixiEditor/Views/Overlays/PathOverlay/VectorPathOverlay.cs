using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.Extensions.UI.Overlays;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Views.Overlays.Drawables;
using PixiEditor.Views.Overlays.Handles;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays.PathOverlay;

// If you need to make any changes in this overlay, I feel sorry for you
public class VectorPathOverlay : Overlay
{
    public static readonly StyledProperty<VectorPath> PathProperty =
        AvaloniaProperty.Register<VectorPathOverlay, VectorPath>(
            nameof(Path));

    public static readonly StyledProperty<SnappingController> SnappingControllerProperty =
        AvaloniaProperty.Register<VectorPathOverlay, SnappingController>(
            nameof(SnappingController));

    public SnappingController SnappingController
    {
        get => GetValue(SnappingControllerProperty);
        set => SetValue(SnappingControllerProperty, value);
    }

    public VectorPath Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    public static readonly StyledProperty<ICommand> AddToUndoCommandProperty =
        AvaloniaProperty.Register<VectorPathOverlay, ICommand>(
            nameof(AddToUndoCommand));

    public ICommand AddToUndoCommand
    {
        get => GetValue(AddToUndoCommandProperty);
        set => SetValue(AddToUndoCommandProperty, value);
    }

    private DashedStroke dashedStroke = new DashedStroke();
    private TransformHandle transformHandle;
    private AnchorHandle insertPreviewHandle;

    private List<AnchorHandle> anchorHandles = new();
    private List<ControlPointHandle> controlPointHandles = new();

    private VecD posOnStartDrag;
    private VectorPath pathOnStartDrag;

    private EditableVectorPath editableVectorPath;
    private bool canInsert = false;

    static VectorPathOverlay()
    {
        AffectsOverlayRender(PathProperty);
        PathProperty.Changed.Subscribe(OnPathChanged);
    }

    public VectorPathOverlay()
    {
        transformHandle = new TransformHandle(this);
        transformHandle.OnPress += MoveHandlePress;
        transformHandle.OnDrag += MoveHandleDrag;

        AddHandle(transformHandle);
        
        insertPreviewHandle = new AnchorHandle(this);
        
        AddHandle(insertPreviewHandle);
    }

    protected override void ZoomChanged(double newZoom)
    {
        dashedStroke.UpdateZoom((float)newZoom);
        transformHandle.ZoomScale = newZoom;
    }

    public override void RenderOverlay(Canvas context, RectD canvasBounds)
    {
        if (Path is null)
        {
            return;
        }

        dashedStroke.Draw(context, Path);

        RenderHandles(context);
        
        if (canInsert)
        {
            insertPreviewHandle.Draw(context);
        }

        if (IsOverAnyHandle() || canInsert)
        {
            TryHighlightSnap(null, null);
        }
    }

    public override bool CanRender()
    {
        return Path != null;
    }

    private void RenderHandles(Canvas context)
    {
        bool anySelected = false;

        EditableVectorPath editablePath = new EditableVectorPath(Path);

        int anchorIndex = 0;
        int controlPointIndex = 0;
        for (int i = 0; i < editablePath.SubShapes.Count; i++)
        {
            var subPath = editablePath.SubShapes[i];

            if (subPath.Points.Count == 0)
            {
                continue;
            }

            foreach (var point in subPath.Points)
            {
                var handle = anchorHandles[anchorIndex];
                handle.Position = (VecD)point.Position;

                if (point.Verb.ControlPoint1 != null || point.Verb.ControlPoint2 != null)
                {
                    DrawControlPoints(context, point, ref controlPointIndex);
                }

                handle.Draw(context);
                anySelected |= handle.IsSelected;
                anchorIndex++;
            }
        }

        transformHandle.Position = Path.TightBounds.BottomRight + new VecD(1, 1);
        transformHandle.Draw(context);
    }

    private void DrawControlPoints(Canvas context, ShapePoint point, ref int controlPointIndex)
    {
        if (point.Verb.VerbType != PathVerb.Cubic) return;

        if (point.Verb.ControlPoint1 != null)
        {
            var controlPoint1 = controlPointHandles[controlPointIndex];
            controlPoint1.HitTestVisible = controlPoint1.Position != controlPoint1.ConnectedTo.Position;
            controlPoint1.Position = (VecD)point.Verb.ControlPoint1;
            if (controlPoint1.HitTestVisible)
            {
                controlPoint1.Draw(context);
            }

            controlPointIndex++;
        }

        if (point.Verb.ControlPoint2 != null)
        {
            var controlPoint2 = controlPointHandles[controlPointIndex];
            controlPoint2.Position = (VecD)point.Verb.ControlPoint2;
            controlPoint2.HitTestVisible = controlPoint2.Position != controlPoint2.ConnectedTo.Position;

            if (controlPoint2.HitTestVisible)
            {
                controlPoint2.Draw(context);
            }

            controlPointIndex++;
        }
    }

    private void AdjustHandles(EditableVectorPath path)
    {
        int pointsCount = path.TotalPoints + path.ControlPointsCount;
        int anchorCount = anchorHandles.Count;
        int totalHandles = anchorCount + controlPointHandles.Count;
        if (totalHandles != pointsCount)
        {
            if (totalHandles > pointsCount)
            {
                RemoveAllHandles();
            }

            int missingControlPoints = path.ControlPointsCount - controlPointHandles.Count;
            int missingAnchors = path.TotalPoints - anchorHandles.Count;
            for (int i = 0; i < missingAnchors; i++)
            {
                CreateHandle(anchorHandles.Count);
            }

            for (int i = 0; i < missingControlPoints; i++)
            {
                CreateHandle(controlPointHandles.Count, true);
            }


            SelectAnchor(GetHandleAt(pointsCount - 1));

            ConnectControlPointsToAnchors();
        }

        Refresh();
    }

    private void ConnectControlPointsToAnchors()
    {
        if (controlPointHandles.Count == 0)
        {
            return;
        }

        int controlPointIndex = 0;
        foreach (var subShape in editableVectorPath.SubShapes)
        {
            foreach (var point in subShape.Points)
            {
                if (point.Verb.VerbType == PathVerb.Cubic)
                {
                    var controlPoint1 = controlPointHandles[controlPointIndex];
                    var controlPoint2 = controlPointHandles[controlPointIndex + 1];

                    var nextPoint = subShape.GetNextPoint(point.Index);

                    int globalIndex = editableVectorPath.GetGlobalIndex(subShape, point.Index);

                    controlPoint1.ConnectedTo = GetHandleAt(globalIndex);

                    if (nextPoint != null)
                    {
                        int globalNextIndex = editableVectorPath.GetGlobalIndex(subShape, nextPoint.Index);
                        controlPoint2.ConnectedTo = GetHandleAt(globalNextIndex);
                    }

                    controlPointIndex += 2;
                }
            }
        }
    }

    private void RemoveAllHandles()
    {
        int previouslySelectedIndex = -1;

        for (int i = anchorHandles.Count - 1; i >= 0; i--)
        {
            var handle = anchorHandles[i];
            handle.OnPress -= OnHandlePress;
            handle.OnDrag -= OnHandleDrag;
            handle.OnRelease -= OnHandleRelease;
            handle.OnTap -= OnHandleTap;
            if (handle is { IsSelected: true })
            {
                previouslySelectedIndex = i;
            }

            Handles.Remove(handle);
        }

        for (int i = controlPointHandles.Count - 1; i >= 0; i--)
        {
            var handle = controlPointHandles[i];
            handle.OnDrag -= OnControlPointDrag;
            handle.OnRelease -= OnHandleRelease;

            Handles.Remove(controlPointHandles[i]);
        }

        anchorHandles.Clear();
        controlPointHandles.Clear();
        SnappingController.RemoveAll("editingPath");
    }

    private bool IsOverAnyHandle()
    {
        return Handles.Any(handle => handle.IsHovered);
    }

    private void MoveHandlePress(Handle source, OverlayPointerArgs args)
    {
        posOnStartDrag = args.Point;
        pathOnStartDrag?.Dispose();
        pathOnStartDrag = new VectorPath(Path);
        TryHighlightSnap(null, null);
        args.Pointer.Capture(this);
        args.Handled = true;
    }


    private void MoveHandleDrag(Handle source, OverlayPointerArgs args)
    {
        var delta = args.Point - posOnStartDrag;

        VectorPath updatedPath = new VectorPath(pathOnStartDrag);

        delta = TryFindAnySnap(delta, pathOnStartDrag, out string axisX, out string axisY);
        updatedPath.Transform(Matrix3X3.CreateTranslation((float)delta.X, (float)delta.Y));

        TryHighlightSnap(axisX, axisY);

        Path = updatedPath;
        args.Handled = true;
    }

    private void CreateHandle(int atIndex, bool isControlPoint = false)
    {
        if (!isControlPoint)
        {
            AnchorHandle anchor = new AnchorHandle(this);
            anchorHandles.Add(anchor);

            anchor.OnPress += OnHandlePress;
            anchor.OnDrag += OnHandleDrag;
            anchor.OnRelease += OnHandleRelease;
            anchor.OnTap += OnHandleTap;
            AddHandle(anchor);
            SnappingController.AddXYAxis($"editingPath[{atIndex}]", () => anchor.Position);
        }
        else
        {
            var controlPoint = new ControlPointHandle(this);
            controlPoint.OnDrag += OnControlPointDrag;
            controlPoint.OnRelease += OnHandleRelease;
            controlPointHandles.Add(controlPoint);
            AddHandle(controlPoint);
        }
    }

    private void OnHandleTap(Handle handle, OverlayPointerArgs args)
    {
        if (handle is not AnchorHandle anchorHandle)
        {
            return;
        }

        if (Path.IsClosed)
        {
            return;
        }

        VectorPath newPath = new VectorPath(Path);
        if (args.Modifiers.HasFlag(KeyModifiers.Control))
        {
            SelectAnchor(anchorHandle);
            return;
        }

        if (anchorHandles.IndexOf(anchorHandle) == 0)
        {
            newPath.LineTo((VecF)anchorHandle.Position);
            newPath.Close();
        }
        else
        {
            VecD pos = anchorHandle.Position;
            newPath.LineTo(new VecF((float)pos.X, (float)pos.Y));
        }

        Path = newPath;
    }

    private void SelectAnchor(AnchorHandle handle)
    {
        foreach (var anchorHandle in anchorHandles)
        {
            anchorHandle.IsSelected = anchorHandle == handle;
        }
    }

    protected override void OnOverlayPointerPressed(OverlayPointerArgs args)
    {
        if(args.Modifiers.HasFlag(KeyModifiers.Shift) && IsOverPath(args.Point, out VecD closestPoint))
        {
            AddPointAt(closestPoint);
        }
    }

    protected override void OnOverlayPointerMoved(OverlayPointerArgs args)
    {
        if(args.Modifiers.HasFlag(KeyModifiers.Shift) && IsOverPath(args.Point, out VecD closestPoint))
        {
            insertPreviewHandle.Position = closestPoint;
            canInsert = true;
        }
        else
        {
            canInsert = false;
        }
    }

    private void AddPointAt(VecD point)
    {
        editableVectorPath.AddPointAt((VecF)point);
        Path = editableVectorPath.ToVectorPath();
    }
    
    private bool IsOverPath(VecD point, out VecD closestPoint)
    {
        VecD? closest = editableVectorPath.GetClosestPointOnPath(point, 20 / (float)ZoomScale);
        closestPoint = closest ?? point;
        
        return closest != null;
    }

    private void OnHandlePress(Handle source, OverlayPointerArgs args)
    {
        if (source is AnchorHandle anchorHandle)
        {
            SnappingController.RemoveAll($"editingPath[{anchorHandles.IndexOf(anchorHandle)}]");
            CaptureHandle(source);

            if (!args.Modifiers.HasFlag(KeyModifiers.Control)) return;

            var newPath = ConvertTouchingVerbsToCubic(anchorHandle);

            int index = anchorHandles.IndexOf(anchorHandle);
            SubShape subShapeContainingIndex = editableVectorPath.GetSubShapeContainingIndex(index);
            int localIndex = editableVectorPath.GetSubShapePointIndex(index, subShapeContainingIndex);

            HandleContinousCubicDrag(anchorHandle.Position, anchorHandle, subShapeContainingIndex, localIndex);

            Path = newPath.ToVectorPath();
        }
    }

    // To have continous spline, verb before and after a point must be a cubic with proper control points
    private EditableVectorPath ConvertTouchingVerbsToCubic(AnchorHandle anchorHandle)
    {
        int index = anchorHandles.IndexOf(anchorHandle);

        SubShape subShapeContainingIndex = editableVectorPath.GetSubShapeContainingIndex(index);

        int localIndex = editableVectorPath.GetSubShapePointIndex(index, subShapeContainingIndex);

        var previousPoint = subShapeContainingIndex.GetPreviousPoint(localIndex);
        var nextPoint = subShapeContainingIndex.Points[localIndex];

        previousPoint?.ConvertVerbToCubic();

        nextPoint.ConvertVerbToCubic();

        return editableVectorPath;
    }

    private void OnHandleDrag(Handle source, OverlayPointerArgs args)
    {
        if (source is not AnchorHandle handle)
        {
            return;
        }

        var index = anchorHandles.IndexOf(handle);

        var targetPos = ApplySnapping(args.Point);

        SubShape subShapeContainingIndex = editableVectorPath.GetSubShapeContainingIndex(index);

        int localIndex = editableVectorPath.GetSubShapePointIndex(index, subShapeContainingIndex);

        bool isDraggingControlPoints = args.Modifiers.HasFlag(KeyModifiers.Control);

        if (isDraggingControlPoints)
        {
            HandleContinousCubicDrag(targetPos, handle, subShapeContainingIndex, localIndex);
        }
        else
        {
            subShapeContainingIndex.SetPointPosition(localIndex, (VecF)targetPos, true);
        }

        Path = editableVectorPath.ToVectorPath();
    }

    private void HandleContinousCubicDrag(VecD targetPos, AnchorHandle handle, SubShape subShapeContainingIndex,
        int localIndex, bool swapOrder = false)
    {
        var symmetricPos = GetMirroredControlPoint((VecF)targetPos, (VecF)handle.Position);

        if (swapOrder)
        {
            (targetPos, symmetricPos) = (symmetricPos, targetPos);
        }

        subShapeContainingIndex.Points[localIndex].Verb.ControlPoint1 = (VecF)targetPos;

        var previousPoint = subShapeContainingIndex.GetPreviousPoint(localIndex);

        if (previousPoint != null)
        {
            previousPoint.Verb.ControlPoint2 = (VecF)symmetricPos;
        }
    }

    private void OnControlPointDrag(Handle source, OverlayPointerArgs args)
    {
        if (source is not ControlPointHandle controlPointHandle ||
            controlPointHandle.ConnectedTo is not AnchorHandle to)
        {
            return;
        }

        var targetPos = ApplySnapping(args.Point);

        var globalIndex = anchorHandles.IndexOf(to);
        var subShapeContainingIndex = editableVectorPath.GetSubShapeContainingIndex(globalIndex);

        int localIndex = editableVectorPath.GetSubShapePointIndex(globalIndex, subShapeContainingIndex);

        bool dragOnlyOne = args.Modifiers.HasFlag(KeyModifiers.Alt);

        if (!dragOnlyOne)
        {
            bool isDraggingFirst = controlPointHandles.IndexOf(controlPointHandle) % 2 == 0;
            HandleContinousCubicDrag(targetPos, to, subShapeContainingIndex, localIndex, !isDraggingFirst);
        }
        else
        {
            bool isFirstControlPoint = controlPointHandles.IndexOf(controlPointHandle) % 2 == 0;
            if (isFirstControlPoint)
            {
                subShapeContainingIndex.Points[localIndex].Verb.ControlPoint1 = (VecF)targetPos;
            }
            else
            {
                var previousPoint = subShapeContainingIndex.GetPreviousPoint(localIndex);
                if (previousPoint != null)
                {
                    previousPoint.Verb.ControlPoint2 = (VecF)targetPos;
                }
            }
        }

        Path = editableVectorPath.ToVectorPath();
    }

    private VecD GetMirroredControlPoint(VecF controlPoint, VecF anchor)
    {
        return new VecD(2 * anchor.X - controlPoint.X, 2 * anchor.Y - controlPoint.Y);
    }

    private VecD ApplySnapping(VecD point)
    {
        var snappedPoint = SnappingController.GetSnapPoint(point, out string axisX, out string axisY);
        var snapped = new VecD((float)snappedPoint.X, (float)snappedPoint.Y);
        TryHighlightSnap(axisX, axisY);
        return snapped;
    }

    private void OnHandleRelease(Handle source, OverlayPointerArgs args)
    {
        AddToUndoCommand.Execute(Path);

        if (source is AnchorHandle anchorHandle)
        {
            SnappingController.AddXYAxis($"editingPath[{anchorHandles.IndexOf(anchorHandle)}]", () => source.Position);
        }

        TryHighlightSnap(null, null);

        Refresh();
    }

    private void TryHighlightSnap(string axisX, string axisY)
    {
        SnappingController.HighlightedXAxis = axisX;
        SnappingController.HighlightedYAxis = axisY;
        SnappingController.HighlightedPoint = null;
    }

    private AnchorHandle? GetHandleAt(int index)
    {
        if (index < 0 || index >= anchorHandles.Count)
        {
            return null;
        }

        return anchorHandles[index];
    }

    private void ClearAnchorHandles()
    {
        foreach (var handle in anchorHandles)
        {
            handle.OnPress -= OnHandlePress;
            handle.OnDrag -= OnHandleDrag;
            handle.OnRelease -= OnHandleRelease;
            handle.OnTap -= OnHandleTap;
            Handles.Remove(handle);
        }

        anchorHandles.Clear();
    }

    private VecD TryFindAnySnap(VecD delta, VectorPath path, out string? axisX, out string? axisY)
    {
        VecD closestSnapDelta = new VecD(double.PositiveInfinity, double.PositiveInfinity);
        axisX = null;
        axisY = null;

        SnappingController.RemoveAll("editingPath");

        foreach (var point in path.Points)
        {
            var snap = SnappingController.GetSnapDeltaForPoint((VecD)point + delta, out string x, out string y);
            if (snap.X < closestSnapDelta.X && !string.IsNullOrEmpty(x))
            {
                closestSnapDelta = new VecD(snap.X, closestSnapDelta.Y);
                axisX = x;
            }

            if (snap.Y < closestSnapDelta.Y && !string.IsNullOrEmpty(y))
            {
                closestSnapDelta = new VecD(closestSnapDelta.X, snap.Y);
                axisY = y;
            }
        }

        AddAllSnaps();

        if (closestSnapDelta.X == double.PositiveInfinity)
        {
            closestSnapDelta = new VecD(0, closestSnapDelta.Y);
        }

        if (closestSnapDelta.Y == double.PositiveInfinity)
        {
            closestSnapDelta = new VecD(closestSnapDelta.X, 0);
        }

        return delta + closestSnapDelta;
    }

    private void AddAllSnaps()
    {
        for (int i = 0; i < anchorHandles.Count; i++)
        {
            var i1 = i;
            SnappingController.AddXYAxis($"editingPath[{i}]", () => anchorHandles[i1].Position);
        }
    }

    private void PathChanged(VectorPath newPath)
    {
        if (editableVectorPath == null)
        {
            editableVectorPath = new EditableVectorPath(newPath);
        }
        else
        {
            editableVectorPath.Path = newPath;
        }

        AdjustHandles(editableVectorPath);
    }

    private static void OnPathChanged(AvaloniaPropertyChangedEventArgs<VectorPath> args)
    {
        var overlay = args.Sender as VectorPathOverlay;
        if (args.NewValue.Value == null)
        {
            overlay.SnappingController.RemoveAll("editingPath");
            overlay.ClearAnchorHandles();
            overlay.IsVisible = false;
            overlay.editableVectorPath = null;
        }
        else
        {
            var path = args.NewValue.Value;
            EditableVectorPath editablePath = new EditableVectorPath(path);
            overlay.editableVectorPath = editablePath;
            overlay.AdjustHandles(editablePath);
            overlay.IsVisible = true;
        }

        if (args.OldValue.Value != null)
        {
            args.OldValue.Value.Changed -= overlay.PathChanged;
        }

        if (args.NewValue.Value != null)
        {
            overlay.editableVectorPath = new EditableVectorPath(args.NewValue.Value);
            args.NewValue.Value.Changed += overlay.PathChanged;
        }
    }
}
