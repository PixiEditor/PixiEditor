using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Helpers;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.Views.Nodes;

public abstract class ResizableNodeView : NodeView
{
    private bool _isResizing;
    private VecI _dragInitialSize;
    private VecD _dragStartGraphPos;
    private ResizeMode _dragMode;
    private ResizeMode _currentHoverMode;
    private const double EdgeMargin = 10.0;
    private const int MinWidth = 160;
    private const int MinHeight = 40;
    private NodeGraphView? _graphView;
    private Border? _rectBorder;

    protected SolidColorBrush? fillBrush;
    protected SolidColorBrush? strokeBrush;

    public double FillOpacity { get; set; } = 1;

    private VecD _dragInitialPosition;


    public static readonly StyledProperty<VecI> BoxSizeProperty =
        AvaloniaProperty.Register<ResizableNodeView, VecI>(
            nameof(BoxSize), defaultBindingMode: BindingMode.TwoWay);

    public VecI BoxSize
    {
        get => GetValue(BoxSizeProperty);
        set => SetValue(BoxSizeProperty, value);
    }


    protected enum ResizeMode
    {
        None,
        Left,
        Right,
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _rectBorder = e.NameScope.Find<Border>("PART_Rectangle");

        if (_rectBorder != null)
        {
            fillBrush = new SolidColorBrush() { Opacity = FillOpacity };
            strokeBrush = new SolidColorBrush();
            _rectBorder.Background = fillBrush;
            _rectBorder.BorderBrush = strokeBrush;
        }

    }


    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.Handled) return;
        if (Node is null || e.GetMouseButton(this) != MouseButton.Left)
        {
            base.OnPointerPressed(e);
            return;
        }

        _graphView ??= this.FindAncestorOfType<NodeGraphView>();
        if (_graphView == null)
        {
            base.OnPointerPressed(e);
            return;
        }

        var graphPos = ScreenToGraph(e, _graphView);
        var mode = HitTest(graphPos);

        if (mode == ResizeMode.None)
        {
            base.OnPointerPressed(e);
            return;
        }

        BeginResize(graphPos, mode);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (Node is null) return;

        _graphView ??= this.FindAncestorOfType<NodeGraphView>();
        if (_graphView == null) return;

        var graphPos = ScreenToGraph(e, _graphView);

        if (_isResizing)
        {
            UpdateResize(graphPos);
            e.Handled = true;
            return;
        }

        var mode = HitTest(graphPos);
        if (mode != _currentHoverMode)
        {
            _currentHoverMode = mode;
            Cursor = new Cursor(CursorFor(mode));
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (_isResizing)
        {
            EndResize();
            _isResizing = false;
            e.Pointer.Capture(null);
            e.Handled = true;
            return;
        }

        base.OnPointerReleased(e);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (_isResizing)
        {
            EndResize();
            _isResizing = false;
            return;
        }

        base.OnPointerCaptureLost(e);
    }

    private ResizeMode HitTest(VecD graphPos)
    {
        if (_rectBorder == null)
            return ResizeMode.None;

        var bounds = _rectBorder.Bounds;
        var pos = Node.PositionBindable;

        var tl = pos + new VecD(bounds.X, bounds.Y);
        var br = tl + new VecD(bounds.Width, bounds.Height);

        bool nearLeft = Math.Abs(graphPos.X - tl.X) <= EdgeMargin;
        bool nearRight = Math.Abs(graphPos.X - br.X) <= EdgeMargin;
        bool nearTop = Math.Abs(graphPos.Y - tl.Y) <= EdgeMargin;
        bool nearBottom = Math.Abs(graphPos.Y - br.Y) <= EdgeMargin;

        if (nearTop && nearLeft) return ResizeMode.TopLeft;
        if (nearTop && nearRight) return ResizeMode.TopRight;
        if (nearBottom && nearLeft) return ResizeMode.BottomLeft;
        if (nearBottom && nearRight) return ResizeMode.BottomRight;

        if (nearLeft) return ResizeMode.Left;
        if (nearRight) return ResizeMode.Right;
        if (nearTop) return ResizeMode.Top;
        if (nearBottom) return ResizeMode.Bottom;

        return ResizeMode.None;
    }

    private void BeginResize(VecD startGraphPos, ResizeMode mode)
    {
        _dragInitialSize = BoxSize;
        _dragInitialPosition = Node.PositionBindable;
        _dragStartGraphPos = startGraphPos;
        _dragMode = mode;
        _isResizing = true;
        SetSizeMergeChanges(true);
    }

    private void UpdateResize(VecD currentGraphPos)
    {
        var delta = currentGraphPos - _dragStartGraphPos;

        int width = _dragInitialSize.X;
        int height = _dragInitialSize.Y;

        double posX = _dragInitialPosition.X;
        double posY = _dragInitialPosition.Y;

        if (_dragMode is ResizeMode.Left or ResizeMode.TopLeft or ResizeMode.BottomLeft)
        {
            width = Math.Max(MinWidth, (int)Math.Round(_dragInitialSize.X - delta.X));
            posX = _dragInitialPosition.X + (_dragInitialSize.X - width);
        }

        if (_dragMode is ResizeMode.Right or ResizeMode.TopRight or ResizeMode.BottomRight)
        {
            width = Math.Max(MinWidth, (int)Math.Round(_dragInitialSize.X + delta.X));
        }

        if (_dragMode is ResizeMode.Top or ResizeMode.TopLeft or ResizeMode.TopRight)
        {
            height = Math.Max(MinHeight, (int)Math.Round(_dragInitialSize.Y - delta.Y));
            posY = _dragInitialPosition.Y + (_dragInitialSize.Y - height);
        }

        if (_dragMode is ResizeMode.Bottom or ResizeMode.BottomLeft or ResizeMode.BottomRight)
        {
            height = Math.Max(MinHeight, (int)Math.Round(_dragInitialSize.Y + delta.Y));
        }

        BoxSize = new VecI(width, height);
        Node.PositionBindable = new VecD(posX, posY);
    }

    private void EndResize()
    {
        SetSizeMergeChanges(false);
    }

    private void SetSizeMergeChanges(bool value)
    {
        if (Node is not NodeViewModel nv) return;
        if (!nv.InputPropertyMap.TryGetValue(StickyNoteNode.SizePropertyName, out var handler)) return;
        if (handler is NodePropertyViewModel propVm) propVm.MergeChanges = value;
    }

    private static VecD ScreenToGraph(PointerEventArgs e, NodeGraphView graphView)
    {
        var screenPos = e.GetPosition(graphView);
        return graphView.ToZoomboxSpace(new VecD(screenPos.X, screenPos.Y));
    }

    private static StandardCursorType CursorFor(ResizeMode mode) => mode switch
    {
        ResizeMode.Left or ResizeMode.Right
            => StandardCursorType.SizeWestEast,

        ResizeMode.Top or ResizeMode.Bottom
            => StandardCursorType.SizeNorthSouth,

        ResizeMode.TopLeft or ResizeMode.BottomRight
            => StandardCursorType.TopLeftCorner,

        ResizeMode.TopRight or ResizeMode.BottomLeft
            => StandardCursorType.TopRightCorner,

        _ => StandardCursorType.Arrow
    };
}
