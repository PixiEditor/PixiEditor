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

    private bool isDragging = false;
    private bool pointerPressed = false;
    private bool convertSelectedOnDrag = false;
    private bool converted = false;

    private List<int> lastSelectedIndices = new();

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
        insertPreviewHandle.HitTestVisible = false;

        AddHandle(insertPreviewHandle);
    }

    protected override void ZoomChanged(double newZoom)
    {
        dashedStroke.UpdateZoom((float)newZoom);
        transformHandle.ZoomScale = newZoom;
    }

    protected override void OnRenderOverlay(Canvas context, RectD canvasBounds)
    {
        if (Path is null)
        {
            return;
        }

        dashedStroke.Draw(context, Path);

        RenderHandles(context);

        if (canInsert && CapturedHandle == null)
        {
            insertPreviewHandle.Draw(context);
        }

        if ((IsOverAnyHandle() && !isDragging) || canInsert)
        {
            TryHighlightSnap(null, null);
        }
    }

    public override bool CanRender()
    {
        return Path is { IsEmpty: false };
    }

    private void RenderHandles(Canvas context)
    {
        bool anySelected = false;

        EditableVectorPath editablePath = new EditableVectorPath(Path);

        UpdatePointsPositions();

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
                if (anchorIndex >= anchorHandles.Count)
                {
                    break;
                }

                var handle = anchorHandles[anchorIndex];
                var nextLocalIndex = subPath.GetNextPoint(point.Index)?.Index ?? 0;
                int nextGlobalIndex = editablePath.GetGlobalIndex(subPath, nextLocalIndex);
                bool nextIsSelected = nextGlobalIndex < anchorHandles.Count &&
                                      anchorHandles[nextGlobalIndex].IsSelected;
                bool drawControl1 = handle.IsSelected;
                bool drawControl2 = nextIsSelected;

                if ((point.Verb.ControlPoint1 != null || point.Verb.ControlPoint2 != null))
                {
                    DrawControlPoints(context, point, drawControl1, drawControl2, ref controlPointIndex);
                }

                handle.Draw(context);
                anySelected |= handle.IsSelected;
                anchorIndex++;
            }
        }

        transformHandle.Position = Path.TightBounds.BottomRight +
                                   new VecD(transformHandle.Size.X / ZoomScale, transformHandle.Size.Y / ZoomScale);
        transformHandle.Draw(context);
    }

    private void DrawControlPoints(Canvas context, ShapePoint point, bool canDrawControl1, bool canDrawControl2, ref int controlPointIndex)
    {
        if (point.Verb.VerbType != PathVerb.Cubic) return;

        if (point.Verb.ControlPoint1 != null)
        {
            var controlPoint1 = controlPointHandles[controlPointIndex];
            controlPoint1.HitTestVisible = CapturedHandle == controlPoint1 ||
                                           controlPoint1.Position != controlPoint1.ConnectedTo.Position;
            controlPoint1.Position = (VecD)point.Verb.ControlPoint1;
            if (controlPoint1.HitTestVisible && canDrawControl1)
            {
                controlPoint1.Draw(context);
            }

            controlPointIndex++;
        }

        if (point.Verb.ControlPoint2 != null)
        {
            var controlPoint2 = controlPointHandles[controlPointIndex];
            controlPoint2.Position = (VecD)point.Verb.ControlPoint2;
            controlPoint2.HitTestVisible = CapturedHandle == controlPoint2 ||
                                           controlPoint2.Position != controlPoint2.ConnectedTo.Position;

            if (controlPoint2.HitTestVisible && canDrawControl2)
            {
                controlPoint2.Draw(context);
            }

            controlPointIndex++;
        }
    }

    public override bool TestHit(VecD point)
    {
        return Path != null;
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

            foreach (var sel in lastSelectedIndices)
            {
                if (sel >= anchorHandles.Count)
                {
                    continue;
                }

                var handle = anchorHandles[sel];
                handle.IsSelected = true;
            }

            ConnectControlPointsToAnchors();
        }

        UpdatePointsPositions();
        Refresh();
    }

    private void UpdatePointsPositions()
    {
        int controlPointIndex = 0;
        int anchorIndex = 0;
        foreach (var subShape in editableVectorPath.SubShapes)
        {
            foreach (var point in subShape.Points)
            {
                if (point.Verb.VerbType == PathVerb.Cubic)
                {
                    var controlPoint1 = controlPointHandles[controlPointIndex];
                    var controlPoint2 = controlPointHandles[controlPointIndex + 1];

                    controlPoint1.Position = (VecD)point.Verb.ControlPoint1;
                    controlPoint2.Position = (VecD)point.Verb.ControlPoint2;

                    controlPointIndex += 2;
                }

                if (anchorIndex >= anchorHandles.Count) continue;

                var anchor = anchorHandles[anchorIndex];
                anchor.Position = (VecD)point.Position;
                anchorIndex++;
            }
        }
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
            handle.OnPress -= OnControlPointPress;
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
        isDragging = true;
    }


    private void MoveHandleDrag(Handle source, OverlayPointerArgs args)
    {
        var delta = args.Point - posOnStartDrag;

        VectorPath updatedPath = new VectorPath(pathOnStartDrag);

        delta = TryFindAnySnap(delta, pathOnStartDrag, out string axisX, out string axisY, out VecD? snapPoint);
        updatedPath.Transform(Matrix3X3.CreateTranslation((float)delta.X, (float)delta.Y));

        TryHighlightSnap(axisX, axisY, snapPoint);

        Path = updatedPath;
        args.Handled = true;
        isDragging = true;
    }

    protected override void OnKeyPressed(KeyEventArgs args)
    {
        if (args.Key == Key.Delete)
        {
            DeleteSelectedPoints();
            args.Handled = true;
        }
    }

    private void DeleteSelectedPoints()
    {
        var selectedHandles = anchorHandles.Where(h => h.IsSelected).ToList();
        if (selectedHandles == null || selectedHandles.Count == 0)
        {
            return;
        }

        int handleAdjustment = 0;

        foreach (var handle in selectedHandles)
        {
            int index = anchorHandles.IndexOf(handle) - handleAdjustment;
            SubShape subShapeContainingIndex = editableVectorPath.GetSubShapeContainingIndex(index);
            int localIndex = editableVectorPath.GetSubShapePointIndex(index, subShapeContainingIndex);

            if (subShapeContainingIndex.Points.Count == 1)
            {
                editableVectorPath.RemoveSubShape(subShapeContainingIndex);
            }
            else
            {
                subShapeContainingIndex.RemovePoint(localIndex);
            }

            handleAdjustment++;
        }

        Path = editableVectorPath.ToVectorPath();
        AddToUndoCommand?.Execute(Path);
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
            SnappingController?.AddXYAxis($"editingPath[{atIndex}]", () =>
            {
                var subs = editableVectorPath.GetSubShapeContainingIndex(atIndex);
                int localIndex = editableVectorPath.GetSubShapePointIndex(atIndex, subs);
                return (VecD)subs.Points[localIndex].Position;
            });
        }
        else
        {
            var controlPoint = new ControlPointHandle(this);
            controlPoint.OnPress += OnControlPointPress;
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

        if (args.Modifiers.HasFlag(KeyModifiers.Control))
        {
            bool append = args.Modifiers.HasFlag(KeyModifiers.Shift);
            SelectAnchor(anchorHandle, append);
            return;
        }

        var selectedHandle = anchorHandles.FirstOrDefault(h => h.IsSelected);
        if (selectedHandle == null)
        {
            return;
        }

        SubShape ssOfSelected = editableVectorPath.GetSubShapeContainingIndex(anchorHandles.IndexOf(selectedHandle));
        SubShape ssOfTapped = editableVectorPath.GetSubShapeContainingIndex(anchorHandles.IndexOf(anchorHandle));

        if (ssOfTapped == null || ssOfSelected == null)
        {
            return;
        }

        int globalIndexOfTapped = anchorHandles.IndexOf(anchorHandle);
        int localIndexOfTapped = editableVectorPath.GetSubShapePointIndex(globalIndexOfTapped, ssOfTapped);

        if (ssOfSelected == ssOfTapped && ssOfTapped.IsClosed)
        {
            return;
        }

        if (ssOfSelected == ssOfTapped && !ssOfTapped.IsClosed &&
            (localIndexOfTapped == 0 || localIndexOfTapped == ssOfTapped.Points.Count - 1))
        {
            ssOfTapped.Close();
        }
        else
        {
            ssOfTapped.AppendPoint((VecF)anchorHandle.Position);
        }

        SelectAnchor(anchorHandle);
        Path = editableVectorPath.ToVectorPath();
    }

    private void SelectAnchor(AnchorHandle handle, bool append = false)
    {
        lastSelectedIndices.Clear();
        if (append)
        {
            handle.IsSelected = !handle.IsSelected;
            return;
        }

        foreach (var anchorHandle in anchorHandles)
        {
            anchorHandle.IsSelected = anchorHandle == handle;
        }
    }

    protected override void OnOverlayPointerPressed(OverlayPointerArgs args)
    {
        if (args.PointerButton != MouseButton.Left)
        {
            return;
        }

        pointerPressed = true;

        if (IsOverPath(args.Point, out VecD closestPoint))
        {
            AddPointAt(closestPoint);
            AddToUndoCommand?.Execute(Path);
            args.Handled = true;
        }
        else if (args.Modifiers == KeyModifiers.None)
        {
            args.Handled = AddNewPointFromClick(SnappingController.GetSnapPoint(args.Point, out _, out _));
            if (args.Handled)
            {
                convertSelectedOnDrag = true;
                converted = false;
            }

            AddToUndoCommand?.Execute(Path);
        }
    }

    protected override void OnOverlayPointerMoved(OverlayPointerArgs args)
    {
        if (pointerPressed && convertSelectedOnDrag)
        {
            var anchor = anchorHandles.FirstOrDefault(h => h.IsSelected);
            if (anchor == null)
            {
                return;
            }

            int index = anchorHandles.IndexOf(anchor);
            var path = editableVectorPath;
            if (!converted)
            {
                path = ConvertTouchingVerbsToCubic(anchor);

                var subshapeContainingIndex = path.GetSubShapeContainingIndex(index);
                int verbIndex = path.GetSubShapePointIndex(index, subshapeContainingIndex);

                if (verbIndex >= 2)
                {
                    var previousPoint = subshapeContainingIndex.GetPreviousPoint(verbIndex);
                    var previousPreviousPoint = subshapeContainingIndex.GetPreviousPoint(verbIndex - 1);

                    if (previousPoint != null && previousPreviousPoint != null &&
                        previousPoint.Verb.VerbType == PathVerb.Cubic &&
                        previousPreviousPoint.Verb.VerbType == PathVerb.Cubic)
                    {
                        var symmetricalPoint = GetMirroredControlPoint(
                            (VecF)previousPreviousPoint.Verb.ControlPoint2, previousPoint.Position);
                        previousPoint.Verb.ControlPoint1 = (VecF)symmetricalPoint;
                    }
                }

                Path = path.ToVectorPath();
                AdjustHandles(path);
                converted = true;
            }

            SubShape subShapeContainingIndex = path.GetSubShapeContainingIndex(index);
            int localIndex = path.GetSubShapePointIndex(index, subShapeContainingIndex);

            HandleContinousCubicDrag(args.Point, subShapeContainingIndex, localIndex, true);
            /*if (localIndex > 0)
            {
                var previousVerb = subShapeContainingIndex.Points[localIndex - 1];
                if (previousVerb?.Verb?.ControlPoint2 != null)
                {
                    var thisVerb = subShapeContainingIndex.Points[localIndex].Verb;
                    thisVerb.ControlPoint1 = (VecF)GetMirroredControlPoint(
                        (VecF)previousVerb.Verb.ControlPoint2, thisVerb.From);
                }
            }*/

            Path = editableVectorPath.ToVectorPath();
        }

        if (IsOverPath(args.Point, out VecD closestPoint))
        {
            insertPreviewHandle.Position = closestPoint;
            canInsert = true;
        }
        else
        {
            canInsert = false;
        }
    }

    protected override void OnOverlayPointerReleased(OverlayPointerArgs args)
    {
        isDragging = false;
        pointerPressed = false;
        convertSelectedOnDrag = false;
        converted = false;
    }

    private bool AddNewPointFromClick(VecD point)
    {
        var selectedHandle = anchorHandles.FirstOrDefault(h => h.IsSelected);
        SubShape subShape = editableVectorPath.GetSubShapeContainingIndex(anchorHandles.IndexOf(selectedHandle));

        if (subShape.IsClosed)
        {
            var path = editableVectorPath.ToVectorPath();
            VectorPath newShape = new VectorPath();
            newShape.MoveTo((VecF)point);
            path.AddPath(newShape, AddPathMode.Append);
            Path = path;
            SelectAnchor(anchorHandles.Last());
            return true;
        }

        if (Path.IsEmpty)
        {
            Path = new VectorPath();
            Path.MoveTo((VecF)point);
            if (anchorHandles.Count > 0)
            {
                SelectAnchor(anchorHandles[0]);
            }
        }
        else
        {
            subShape.AppendPoint((VecF)point);
            Path = editableVectorPath.ToVectorPath();
            SelectAnchor(anchorHandles.Last());
        }

        return true;
    }

    private void AddPointAt(VecD point)
    {
        int? insertedAt = editableVectorPath.AddPointAt(point);
        Path = editableVectorPath.ToVectorPath();
        SelectAnchor(insertedAt is > 0 && insertedAt.Value < anchorHandles.Count
            ? anchorHandles[insertedAt.Value]
            : anchorHandles.Last());
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
            canInsert = false;
            args.Handled = true;
        }
    }

    // To have continous spline, verb before and after a point must be a cubic with proper control points
    private EditableVectorPath ConvertTouchingVerbsToCubic(AnchorHandle anchorHandle)
    {
        int index = anchorHandles.IndexOf(anchorHandle);

        return ConvertTouchingVerbsToCubic(index);
    }

    private EditableVectorPath ConvertTouchingVerbsToCubic(int index)
    {
        SubShape subShapeContainingIndex = editableVectorPath.GetSubShapeContainingIndex(index);

        int localIndex = editableVectorPath.GetSubShapePointIndex(index, subShapeContainingIndex);

        var previousPoint = subShapeContainingIndex.GetPreviousPoint(localIndex);
        var point = subShapeContainingIndex.Points[localIndex];

        previousPoint?.ConvertVerbToCubic();

        point.ConvertVerbToCubic();

        return editableVectorPath;
    }

    private void OnHandleDrag(Handle source, OverlayPointerArgs args)
    {
        if (source is not AnchorHandle handle)
        {
            return;
        }

        if (!handle.IsSelected)
        {
            SelectAnchor(handle, false);
        }

        bool isDraggingControlPoints = args.Modifiers.HasFlag(KeyModifiers.Control);

        var selectedAnchors = isDraggingControlPoints
            ? new List<AnchorHandle>() { handle }
            : anchorHandles.Where(h => h.IsSelected).OrderByDescending(h => h == handle).ToList();

        var targetPos = ApplySnapping(args.Point);
        VecF delta = VecF.Zero;

        for (int i = 0; i < selectedAnchors.Count; i++)
        {
            var anchor = selectedAnchors[i];
            var index = anchorHandles.IndexOf(anchor);

            SubShape subShapeContainingIndex = editableVectorPath.GetSubShapeContainingIndex(index);

            int localIndex = editableVectorPath.GetSubShapePointIndex(index, subShapeContainingIndex);

            if (isDraggingControlPoints)
            {
                var newPath = ConvertTouchingVerbsToCubic(anchor);

                subShapeContainingIndex = newPath.GetSubShapeContainingIndex(index);
                localIndex = newPath.GetSubShapePointIndex(index, subShapeContainingIndex);

                HandleContinousCubicDrag(targetPos, subShapeContainingIndex, localIndex, true);
            }
            else
            {
                VecF pos = (VecF)subShapeContainingIndex.Points[localIndex].Position;
                if (i == 0)
                {
                    delta = (VecF)targetPos - pos;
                }

                VecF newPos = i == 0 ? (VecF)targetPos : pos + delta;
                subShapeContainingIndex.SetPointPosition(localIndex, newPos, true);
            }
        }

        Path = editableVectorPath.ToVectorPath();
    }

    private void HandleContinousCubicDrag(VecD targetPos, SubShape subShapeContainingIndex,
        int localIndex, bool constrainRatio, bool swapOrder = false)
    {
        var previousPoint = subShapeContainingIndex.GetPreviousPoint(localIndex);

        VecD symmetricPos = targetPos;
        bool canMirror = true;

        var thisPoint = subShapeContainingIndex.Points[localIndex];

        if (constrainRatio)
        {
            symmetricPos = GetMirroredControlPoint((VecF)targetPos, thisPoint.Position);
        }
        else
        {
            VecD direction = targetPos - (VecD)thisPoint.Position;
            direction = direction.Normalize();
            var controlPos = ((VecD?)previousPoint?.Verb.ControlPoint2 ?? targetPos);
            if (swapOrder)
            {
                controlPos = ((VecD?)subShapeContainingIndex.Points[localIndex]?.Verb.ControlPoint1 ?? targetPos);
            }

            double length = VecD.Distance((VecD)thisPoint.Position, controlPos);
            if (!direction.IsNaNOrInfinity())
            {
                symmetricPos = (VecD)thisPoint.Position - direction * length;
            }
            else
            {
                canMirror = false;
            }
        }

        if (swapOrder)
        {
            (targetPos, symmetricPos) = (symmetricPos, targetPos);
        }

        subShapeContainingIndex.Points[localIndex].Verb.ControlPoint1 = (VecF)targetPos;

        if (previousPoint != null && canMirror)
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
            bool constrainRatio = args.Modifiers.HasFlag(KeyModifiers.Control);
            HandleContinousCubicDrag(targetPos, subShapeContainingIndex, localIndex, constrainRatio,
                !isDraggingFirst);
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
        TryHighlightSnap(axisX, axisY, snapped);
        return snapped;
    }

    private void OnControlPointPress(Handle source, OverlayPointerArgs args)
    {
        CaptureHandle(source);
    }

    private void OnHandleRelease(Handle source, OverlayPointerArgs args)
    {
        AddToUndoCommand?.Execute(Path);

        if (source is AnchorHandle anchorHandle)
        {
            SnappingController.AddXYAxis($"editingPath[{anchorHandles.IndexOf(anchorHandle)}]", () => source.Position);
        }

        TryHighlightSnap(null, null);

        Refresh();
    }

    private void TryHighlightSnap(string axisX, string axisY, VecD? point = null)
    {
        SnappingController.HighlightedXAxis = axisX;
        SnappingController.HighlightedYAxis = axisY;
        SnappingController.HighlightedPoint = point;
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
        lastSelectedIndices.Clear();
        int index = 0;
        foreach (var handle in anchorHandles)
        {
            if (handle.IsSelected)
            {
                lastSelectedIndices.Add(index);
            }

            handle.OnPress -= OnHandlePress;
            handle.OnDrag -= OnHandleDrag;
            handle.OnRelease -= OnHandleRelease;
            handle.OnTap -= OnHandleTap;
            Handles.Remove(handle);
            index++;
        }

        anchorHandles.Clear();
        foreach (var handle in controlPointHandles)
        {
            handle.OnPress -= OnControlPointPress;
            handle.OnDrag -= OnControlPointDrag;
            handle.OnRelease -= OnHandleRelease;
            Handles.Remove(handle);
        }

        controlPointHandles.Clear();
    }

    private VecD TryFindAnySnap(VecD delta, VectorPath path, out string? axisX, out string? axisY, out VecD? snapPoint)
    {
        VecD closestSnapDelta = new VecD(double.PositiveInfinity, double.PositiveInfinity);
        axisX = null;
        axisY = null;
        snapPoint = null;

        SnappingController.RemoveAll("editingPath");

        foreach (var point in path.Points)
        {
            var snap = SnappingController.GetSnapDeltaForPoint((VecD)point + delta, out string x, out string y);
            if (snap.X < closestSnapDelta.X && !string.IsNullOrEmpty(x))
            {
                closestSnapDelta = new VecD(snap.X, closestSnapDelta.Y);
                axisX = x;
                snapPoint = (VecD)point;
            }

            if (snap.Y < closestSnapDelta.Y && !string.IsNullOrEmpty(y))
            {
                closestSnapDelta = new VecD(closestSnapDelta.X, snap.Y);
                axisY = y;
                snapPoint = (VecD)point;
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

        if (snapPoint != null)
        {
            snapPoint = snapPoint + delta + closestSnapDelta;
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
