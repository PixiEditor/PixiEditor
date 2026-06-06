using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using ColorPicker;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Events;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Nodes;
using DrawieColor = Drawie.Backend.Core.ColorsImpl.Color;

namespace PixiEditor.Views.Nodes;

[TemplatePart("PART_Rectangle", typeof(Border))]
[TemplatePart("PART_ColorPicker", typeof(PortableColorPicker))]
public class StickyNoteNodeView : NodeView
{
    private const double EdgeMargin = 10.0;
    private const int MinWidth = 160;
    private const int MinHeight = 40;

    private enum ResizeMode { None, Right, Bottom, BottomRight }

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<StickyNoteNodeView, string>(
            nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<VecI> BoxSizeProperty =
        AvaloniaProperty.Register<StickyNoteNodeView, VecI>(
            nameof(BoxSize), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<IBrush> ForegroundBrushProperty =
        AvaloniaProperty.Register<StickyNoteNodeView, IBrush>(
            nameof(ForegroundBrush), Brushes.Black);

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public VecI BoxSize
    {
        get => GetValue(BoxSizeProperty);
        set => SetValue(BoxSizeProperty, value);
    }

    public IBrush ForegroundBrush
    {
        get => GetValue(ForegroundBrushProperty);
        set => SetValue(ForegroundBrushProperty, value);
    }

    private Border? _rectBorder;
    private PortableColorPicker? _picker;
    private SolidColorBrush? _fillBrush;
    private SolidColorBrush? _strokeBrush;

    private INodePropertyHandler? _colorHandler;
    private bool _updatingPicker;

    private NodeGraphView? _graphView;
    private bool _isResizing;
    private VecI _dragInitialSize;
    private VecD _dragStartGraphPos;
    private ResizeMode _dragMode;
    private ResizeMode _currentHoverMode;

    static StickyNoteNodeView()
    {
        NodeProperty.Changed.Subscribe(OnNodeChanged);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Parent is Visual parent)
            parent.ZIndex = 1;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _rectBorder = e.NameScope.Find<Border>("PART_Rectangle");
        _picker = e.NameScope.Find<PortableColorPicker>("PART_ColorPicker");

        if (_rectBorder != null)
        {
            _fillBrush = new SolidColorBrush();
            _strokeBrush = new SolidColorBrush();
            _rectBorder.Background = _fillBrush;
            _rectBorder.BorderBrush = _strokeBrush;
        }

        if (_picker != null)
            _picker.PropertyChanged += OnPickerPropertyChanged;

        UpdateColorsFromHandler();
    }

    private static void OnNodeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is not StickyNoteNodeView view) return;
        view.UpdateNodeBinding(e.OldValue as INodeHandler, e.NewValue as INodeHandler);
    }

    private void UpdateNodeBinding(INodeHandler? oldNode, INodeHandler? newNode)
    {
        if (_colorHandler != null)
        {
            _colorHandler.ValueChanged -= OnColorValueChanged;
            _colorHandler = null;
        }

        if (newNode != null &&
            newNode.InputPropertyMap.TryGetValue(StickyNoteNode.ColorPropertyName, out var handler))
        {
            _colorHandler = handler;
            _colorHandler.ValueChanged += OnColorValueChanged;
        }

        UpdateColorsFromHandler();
    }

    private void OnColorValueChanged(INodePropertyHandler property, NodePropertyValueChangedArgs args)
    {
        UpdateColorsFromHandler();
    }

    private void UpdateColorsFromHandler()
    {
        if (_colorHandler?.Value is not DrawieColor drawie) return;

        var avalonia = drawie.ToColor();
        if (_fillBrush != null) _fillBrush.Color = avalonia;
        if (_strokeBrush != null) _strokeBrush.Color = ShadedBorder(avalonia);
        ForegroundBrush = ContrastForeground(avalonia);

        if (_picker != null)
        {
            _updatingPicker = true;
            _picker.SelectedColor = avalonia;
            _updatingPicker = false;
        }
    }

    private void OnPickerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_updatingPicker) return;
        if (e.Property != PickerControlBase.SelectedColorProperty) return;
        if (_colorHandler == null || e.NewValue is not Color newColor) return;

        _colorHandler.Value = new DrawieColor(newColor.R, newColor.G, newColor.B, newColor.A);
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
        if (_rectBorder == null) return ResizeMode.None;

        var bounds = _rectBorder.Bounds;
        var pos = Node.PositionBindable;
        var tl = pos + new VecD(bounds.X, bounds.Y);
        var br = tl + new VecD(bounds.Width, bounds.Height);

        if (graphPos.X < tl.X || graphPos.X > br.X + EdgeMargin ||
            graphPos.Y < tl.Y || graphPos.Y > br.Y + EdgeMargin)
        {
            return ResizeMode.None;
        }

        bool nearRight = Math.Abs(graphPos.X - br.X) <= EdgeMargin;
        bool nearBottom = Math.Abs(graphPos.Y - br.Y) <= EdgeMargin;

        if (nearBottom && nearRight) return ResizeMode.BottomRight;
        if (nearBottom) return ResizeMode.Bottom;
        if (nearRight) return ResizeMode.Right;
        return ResizeMode.None;
    }

    private void BeginResize(VecD startGraphPos, ResizeMode mode)
    {
        _dragInitialSize = BoxSize;
        _dragStartGraphPos = startGraphPos;
        _dragMode = mode;
        _isResizing = true;
        SetSizeMergeChanges(true);
    }

    private void UpdateResize(VecD currentGraphPos)
    {
        var delta = currentGraphPos - _dragStartGraphPos;

        int newWidth = _dragInitialSize.X;
        int newHeight = _dragInitialSize.Y;

        if (_dragMode is ResizeMode.Right or ResizeMode.BottomRight)
            newWidth = Math.Max(MinWidth, (int)Math.Round(_dragInitialSize.X + delta.X));

        if (_dragMode is ResizeMode.Bottom or ResizeMode.BottomRight)
            newHeight = Math.Max(MinHeight, (int)Math.Round(_dragInitialSize.Y + delta.Y));

        BoxSize = new VecI(newWidth, newHeight);
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
        ResizeMode.Right => StandardCursorType.SizeWestEast,
        ResizeMode.Bottom => StandardCursorType.SizeNorthSouth,
        ResizeMode.BottomRight => StandardCursorType.TopLeftCorner,
        _ => StandardCursorType.Arrow
    };

    private static IBrush ContrastForeground(Color bg)
    {
        double lum = (0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B) / 255.0;
        return lum > 0.5 ? Brushes.Black : Brushes.White;
    }

    private static Color ShadedBorder(Color bg)
    {
        double lum = (0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B) / 255.0;
        double factor = lum > 0.5 ? 0.65 : 1.5;
        return Color.FromArgb(bg.A,
            (byte)Math.Clamp(bg.R * factor, 0, 255),
            (byte)Math.Clamp(bg.G * factor, 0, 255),
            (byte)Math.Clamp(bg.B * factor, 0, 255));
    }
}
