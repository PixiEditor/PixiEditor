using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.Extensions.UI.Overlays;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Views.Overlays.Drawables;
using PixiEditor.Views.Overlays.Handles;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays.PathOverlay;

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

    private List<AnchorHandle> anchorHandles = new();

    private VecD posOnStartDrag;
    private VectorPath pathOnStartDrag;

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

        AdjustHandles(Path.VerbCount);
        RenderHandles(context);

        if (IsOverAnyHandle())
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
        int i = 0;
        foreach (var verb in Path)
        {
            if (i == Path.VerbCount - 1 && !anySelected)
            {
                GetHandleAt(i).IsSelected = true;
            }

            anySelected = anySelected || GetHandleAt(i).IsSelected;

            VecF verbPointPos = GetVerbPointPos(verb);
            anchorHandles[i].Position = new VecD(verbPointPos.X, verbPointPos.Y);
            anchorHandles[i].Draw(context);
            
            i++;
        }

        transformHandle.Position = Path.TightBounds.BottomRight + new VecD(1, 1);
        transformHandle.Draw(context);
    }

    private void AdjustHandles(int verbCount)
    {
        int anchorCount = anchorHandles.Count;
        if (anchorCount != verbCount)
        {
            if (anchorCount > verbCount)
            {
                RecreateHandles(verbCount);
            }
            else
            {
                for (int i = anchorCount; i < verbCount; i++)
                {
                    CreateHandle(i);
                }

                SelectAnchor(GetHandleAt(verbCount - 1));
            }
        }
    }

    private void RecreateHandles(int points)
    {
        int previouslySelectedIndex = -1;

        for (int i = anchorHandles.Count - 1; i >= 0; i--)
        {
            var handle = anchorHandles[i];
            handle.OnPress -= OnHandlePress;
            handle.OnDrag -= OnHandleDrag;
            handle.OnRelease -= OnHandleRelease;
            handle.OnTap -= OnHandleTap;
            if (handle.IsSelected)
            {
                previouslySelectedIndex = i;
            }

            Handles.Remove(handle);
        }

        anchorHandles.Clear();
        SnappingController.RemoveAll("editingPath");

        for (int i = 0; i < points; i++)
        {
            CreateHandle(i);
            if (i == previouslySelectedIndex)
            {
                GetHandleAt(i).IsSelected = true;
            }
        }
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

    private void CreateHandle(int atIndex)
    {
        var handle = new AnchorHandle(this);
        handle.OnPress += OnHandlePress;
        handle.OnDrag += OnHandleDrag;
        handle.OnRelease += OnHandleRelease;
        handle.OnTap += OnHandleTap;

        anchorHandles.Add(handle);
        AddHandle(handle);

        SnappingController.AddXYAxis($"editingPath[{atIndex}]", () => handle.Position);
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

        if (IsFirstHandle(anchorHandle))
        {
            newPath.Close();
        }
        else
        {
            VecD pos = anchorHandle.Position;
            newPath.LineTo(new VecF((float)pos.X, (float)pos.Y));
        }

        Path = newPath;
    }

    private bool IsFirstHandle(AnchorHandle handle)
    {
        return anchorHandles.IndexOf(handle) == 0;
    }

    private void SelectAnchor(AnchorHandle handle)
    {
        foreach (var anchorHandle in anchorHandles)
        {
            anchorHandle.IsSelected = anchorHandle == handle;
        }
    }

    private void OnHandleDrag(Handle source, OverlayPointerArgs args)
    {
        if (source is not AnchorHandle handle)
        {
            return;
        }

        var index = anchorHandles.IndexOf(handle);
        VectorPath newPath = new VectorPath();

        bool pointHandled = false;
        int i = 0;

        foreach (var data in Path)
        {
            VecF point;
            switch (data.verb)
            {
                case PathVerb.Move:
                    point = data.points[0];
                    point = TryApplyNewPos(args, i, index, point);

                    newPath.MoveTo(point);
                    i++;
                    break;
                case PathVerb.Line:
                    point = data.points[1];
                    point = TryApplyNewPos(args, i, index, point);

                    if (i == index && args.Modifiers.HasFlag(KeyModifiers.Control))
                    {
                        newPath.QuadTo(point, data.points[1]);
                    }
                    else
                    {
                        newPath.LineTo(point);
                    }

                    i++;
                    break;
                case PathVerb.Quad:
                    point = data.points[2];
                    point = TryApplyNewPos(args, i, index, point);

                    if (i == index && args.Modifiers.HasFlag(KeyModifiers.Control))
                    {
                        newPath.QuadTo(point, data.points[2]);
                    }
                    else
                    {
                        newPath.QuadTo(data.points[1], point);
                    }

                    i++;
                    break;
                /*case PathVerb.Quad:
                    newPath.QuadTo(data.points[1], data.points[2]);
                    break;*/
                /*
                case PathVerb.Conic:
                    newPath.ConicTo(data.points[0], point, data.points[2].X);
                    break;
              */
                case PathVerb.Close:
                    newPath.Close();
                    i++;
                    break;
                case PathVerb.Done:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        Path = newPath;
    }
    
    private VecF GetVerbPointPos((PathVerb verb, VecF[] points) data)
    {
        switch (data.verb)
        {
            case PathVerb.Move:
                return data.points[0];
            case PathVerb.Line:
                return data.points[1];
            case PathVerb.Quad:
                return data.points[2];
            case PathVerb.Close:
                return data.points[0];
            case PathVerb.Done:
                return new VecF();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    ///     Gets a point on the opposite side of the anchor point with the same length.
    /// </summary>
    /// <param name="point"></param>
    /// <param name="anchor"></param>
    /// <returns></returns>
    private VecF GetMirroredPoint(VecF point, VecF anchor)
    {
        VecF delta = point - anchor;
        return new VecF(anchor.X - delta.X, anchor.Y - delta.Y);
    }

    private VecF TryApplyNewPos(OverlayPointerArgs args, int i, int index, VecF point)
    {
        if (i == index)
        {
            var snappedPoint = SnappingController.GetSnapPoint(args.Point, out string axisX, out string axisY);
            point = new VecF((float)snappedPoint.X, (float)snappedPoint.Y);
            TryHighlightSnap(axisX, axisY);
        }

        return point;
    }

    private void OnHandlePress(Handle source, OverlayPointerArgs args)
    {
        if (source is AnchorHandle anchorHandle)
        {
            SnappingController.RemoveAll($"editingPath[{anchorHandles.IndexOf(anchorHandle)}]");
        }
    }

    private void OnHandleRelease(Handle source, OverlayPointerArgs args)
    {
        if (source is not AnchorHandle anchorHandle)
        {
            return;
        }

        AddToUndoCommand.Execute(Path);

        SnappingController.AddXYAxis($"editingPath[{anchorHandles.IndexOf(anchorHandle)}]", () => source.Position);

        SnappingController.HighlightedXAxis = null;
        SnappingController.HighlightedYAxis = null;

        Refresh();
    }

    private void TryHighlightSnap(string axisX, string axisY)
    {
        SnappingController.HighlightedXAxis = axisX;
        SnappingController.HighlightedYAxis = axisY;
        SnappingController.HighlightedPoint = null;
    }

    private AnchorHandle GetHandleAt(int index)
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

    private static void OnPathChanged(AvaloniaPropertyChangedEventArgs<VectorPath> args)
    {
        if (args.NewValue.Value == null)
        {
            var overlay = args.Sender as VectorPathOverlay;
            overlay.SnappingController.RemoveAll("editingPath");
            overlay.ClearAnchorHandles();
        }
    }
}
