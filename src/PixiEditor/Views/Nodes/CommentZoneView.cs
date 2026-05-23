using Avalonia.Input;
using Avalonia.VisualTree;
using Drawie.Numerics;
using PixiEditor.Helpers;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.Views.Nodes;

public class CommentZoneView : NodeFrameView
{
    private bool isDragging;
    private NodeGraphView? graphView;
    private CommentZoneDragMode currentHoverMode = CommentZoneDragMode.None;

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetMouseButton(this) != MouseButton.Left)
            return;

        if (DataContext is not CommentZoneViewModel zone)
            return;

        graphView = this.FindAncestorOfType<NodeGraphView>();
        if (graphView == null)
            return;

        var graphPos = ScreenToGraph(e, graphView);
        var mode = zone.HitTest(graphPos);
        if (mode == CommentZoneDragMode.None)
            return;

        zone.BeginDrag(graphPos, mode);
        isDragging = true;
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (DataContext is not CommentZoneViewModel zone)
            return;

        graphView ??= this.FindAncestorOfType<NodeGraphView>();
        if (graphView == null)
            return;

        var graphPos = ScreenToGraph(e, graphView);

        if (isDragging)
        {
            zone.UpdateDrag(graphPos);
            e.Handled = true;
            return;
        }

        var mode = zone.HitTest(graphPos);
        if (mode != currentHoverMode)
        {
            currentHoverMode = mode;
            Cursor = new Cursor(CursorFor(mode));
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!isDragging || DataContext is not CommentZoneViewModel zone)
            return;

        zone.EndDrag();
        isDragging = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        if (!isDragging || DataContext is not CommentZoneViewModel zone)
            return;

        zone.EndDrag();
        isDragging = false;
    }

    private static VecD ScreenToGraph(PointerEventArgs e, NodeGraphView graphView)
    {
        var screenPos = e.GetPosition(graphView);
        return graphView.ToZoomboxSpace(new VecD(screenPos.X, screenPos.Y));
    }

    private static StandardCursorType CursorFor(CommentZoneDragMode mode) => mode switch
    {
        CommentZoneDragMode.Top or CommentZoneDragMode.Bottom => StandardCursorType.SizeNorthSouth,
        CommentZoneDragMode.Left or CommentZoneDragMode.Right => StandardCursorType.SizeWestEast,
        CommentZoneDragMode.TopLeft or CommentZoneDragMode.BottomRight => StandardCursorType.TopLeftCorner,
        CommentZoneDragMode.TopRight or CommentZoneDragMode.BottomLeft => StandardCursorType.TopRightCorner,
        CommentZoneDragMode.Move => StandardCursorType.SizeAll,
        _ => StandardCursorType.Arrow
    };
}
