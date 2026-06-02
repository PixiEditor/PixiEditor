using Avalonia;
using Avalonia.Controls;
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

public class CommentNodeView : NodeView
{
    private const double EdgeMargin = 10.0;
    private const int MinWidth = 160;
    private const int MinHeight = 28;

    private enum ResizeMode { None, Right, Bottom, BottomRight }

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<CommentNodeView, string>(
            nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<VecI> BoxSizeProperty =
        AvaloniaProperty.Register<CommentNodeView, VecI>(
            nameof(BoxSize), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<IBrush> ForegroundBrushProperty =
        AvaloniaProperty.Register<CommentNodeView, IBrush>(
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

    private Border? rectBorder;
    private PortableColorPicker? picker;
    private SolidColorBrush? fillBrush;
    private SolidColorBrush? strokeBrush;

    private INodePropertyHandler? colorHandler;
    private bool updatingPicker;

    private NodeGraphView? graphView;
    private bool isResizing;
    private VecI dragInitialSize;
    private VecD dragStartGraphPos;
    private ResizeMode dragMode;
    private ResizeMode currentHoverMode;

    static CommentNodeView()
    {
        NodeProperty.Changed.AddClassHandler<CommentNodeView>((view, e) =>
            view.OnNodeChanged(e.OldValue as INodeHandler, e.NewValue as INodeHandler));
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Parent is Visual parent)
            parent.ZIndex = -1;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        rectBorder = e.NameScope.Find<Border>("PART_Rectangle");
        picker = e.NameScope.Find<PortableColorPicker>("PART_ColorPicker");

        if (rectBorder != null)
        {
            fillBrush = new SolidColorBrush { Opacity = 0.5 };
            strokeBrush = new SolidColorBrush();
            rectBorder.Background = fillBrush;
            rectBorder.BorderBrush = strokeBrush;
        }

        if (picker != null)
            picker.PropertyChanged += OnPickerPropertyChanged;

        UpdateColorsFromHandler();
    }

    private void OnNodeChanged(INodeHandler? oldNode, INodeHandler? newNode)
    {
        if (colorHandler != null)
        {
            colorHandler.ValueChanged -= OnColorValueChanged;
            colorHandler = null;
        }

        if (newNode != null &&
            newNode.InputPropertyMap.TryGetValue(CommentNode.ColorPropertyName, out var handler))
        {
            colorHandler = handler;
            colorHandler.ValueChanged += OnColorValueChanged;
        }

        UpdateColorsFromHandler();
    }

    private void OnColorValueChanged(INodePropertyHandler property, NodePropertyValueChangedArgs args)
    {
        UpdateColorsFromHandler();
    }

    private void UpdateColorsFromHandler()
    {
        if (colorHandler?.Value is not DrawieColor drawie) return;

        var avalonia = drawie.ToColor();
        if (fillBrush != null) fillBrush.Color = avalonia;
        if (strokeBrush != null) strokeBrush.Color = avalonia;
        ForegroundBrush = ContrastForeground(avalonia);

        if (picker != null)
        {
            updatingPicker = true;
            picker.SelectedColor = avalonia;
            updatingPicker = false;
        }
    }

    private void OnPickerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (updatingPicker) return;
        if (e.Property != PickerControlBase.SelectedColorProperty) return;
        if (colorHandler == null || e.NewValue is not Color newColor) return;

        colorHandler.Value = new DrawieColor(newColor.R, newColor.G, newColor.B, newColor.A);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.Handled) return;
        if (Node is null || e.GetMouseButton(this) != MouseButton.Left)
        {
            base.OnPointerPressed(e);
            return;
        }

        graphView ??= this.FindAncestorOfType<NodeGraphView>();
        if (graphView == null)
        {
            base.OnPointerPressed(e);
            return;
        }

        var graphPos = ScreenToGraph(e, graphView);
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

        graphView ??= this.FindAncestorOfType<NodeGraphView>();
        if (graphView == null) return;

        var graphPos = ScreenToGraph(e, graphView);

        if (isResizing)
        {
            UpdateResize(graphPos);
            e.Handled = true;
            return;
        }

        var mode = HitTest(graphPos);
        if (mode != currentHoverMode)
        {
            currentHoverMode = mode;
            Cursor = new Cursor(CursorFor(mode));
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (isResizing)
        {
            EndResize();
            isResizing = false;
            e.Pointer.Capture(null);
            e.Handled = true;
            return;
        }

        base.OnPointerReleased(e);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (isResizing)
        {
            EndResize();
            isResizing = false;
            return;
        }

        base.OnPointerCaptureLost(e);
    }

    private ResizeMode HitTest(VecD graphPos)
    {
        if (rectBorder == null) return ResizeMode.None;

        var bounds = rectBorder.Bounds;
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
        dragInitialSize = BoxSize;
        dragStartGraphPos = startGraphPos;
        dragMode = mode;
        isResizing = true;
        SetSizeMergeChanges(true);
    }

    private void UpdateResize(VecD currentGraphPos)
    {
        var delta = currentGraphPos - dragStartGraphPos;

        int newWidth = dragInitialSize.X;
        int newHeight = dragInitialSize.Y;

        if (dragMode is ResizeMode.Right or ResizeMode.BottomRight)
            newWidth = Math.Max(MinWidth, (int)Math.Round(dragInitialSize.X + delta.X));

        if (dragMode is ResizeMode.Bottom or ResizeMode.BottomRight)
            newHeight = Math.Max(MinHeight, (int)Math.Round(dragInitialSize.Y + delta.Y));

        BoxSize = new VecI(newWidth, newHeight);
    }

    private void EndResize()
    {
        SetSizeMergeChanges(false);
    }

    private void SetSizeMergeChanges(bool value)
    {
        if (Node is not NodeViewModel nv) return;
        if (!nv.InputPropertyMap.TryGetValue(CommentNode.SizePropertyName, out var handler)) return;
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
}
