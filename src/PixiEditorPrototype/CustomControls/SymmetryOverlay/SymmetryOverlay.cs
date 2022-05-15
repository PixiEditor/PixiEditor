using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditorPrototype.CustomControls.SymmetryOverlay;

internal class SymmetryOverlay : Control
{
    public static readonly DependencyProperty HorizontalAxisYProperty =
        DependencyProperty.Register(nameof(HorizontalAxisY), typeof(int), typeof(SymmetryOverlay),
            new(0, OnPositionUpdate));

    public int HorizontalAxisY
    {
        get => (int)GetValue(HorizontalAxisYProperty);
        set => SetValue(HorizontalAxisYProperty, value);
    }

    public static readonly DependencyProperty VerticalAxisXProperty =
        DependencyProperty.Register(nameof(VerticalAxisX), typeof(int), typeof(SymmetryOverlay),
            new(0, OnPositionUpdate));

    public int VerticalAxisX
    {
        get => (int)GetValue(VerticalAxisXProperty);
        set => SetValue(VerticalAxisXProperty, value);
    }

    public static readonly DependencyProperty HorizontalAxisVisibleProperty =
        DependencyProperty.Register(nameof(HorizontalAxisVisible), typeof(bool), typeof(SymmetryOverlay),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public bool HorizontalAxisVisible
    {
        get => (bool)GetValue(HorizontalAxisVisibleProperty);
        set => SetValue(HorizontalAxisVisibleProperty, value);
    }

    public static readonly DependencyProperty VerticalAxisVisibleProperty =
        DependencyProperty.Register(nameof(VerticalAxisVisible), typeof(bool), typeof(SymmetryOverlay),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public bool VerticalAxisVisible
    {
        get => (bool)GetValue(VerticalAxisVisibleProperty);
        set => SetValue(VerticalAxisVisibleProperty, value);
    }

    public static readonly DependencyProperty ZoomboxScaleProperty =
        DependencyProperty.Register(nameof(ZoomboxScale), typeof(double), typeof(SymmetryOverlay),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public double ZoomboxScale
    {
        get => (double)GetValue(ZoomboxScaleProperty);
        set => SetValue(ZoomboxScaleProperty, value);
    }

    public static readonly DependencyProperty DragCommandProperty =
        DependencyProperty.Register(nameof(DragCommand), typeof(ICommand), typeof(SymmetryOverlay), new(null));

    public ICommand? DragCommand
    {
        get => (ICommand)GetValue(DragCommandProperty);
        set => SetValue(DragCommandProperty, value);
    }

    public static readonly DependencyProperty DragEndCommandProperty =
        DependencyProperty.Register(nameof(DragEndCommand), typeof(ICommand), typeof(SymmetryOverlay), new(null));

    public ICommand? DragEndCommand
    {
        get => (ICommand)GetValue(DragEndCommandProperty);
        set => SetValue(DragEndCommandProperty, value);
    }

    private const double HandleSize = 16;
    private PathGeometry handleGeometry = new()
    {
        FillRule = FillRule.Nonzero,
        Figures = (PathFigureCollection?)new PathFigureCollectionConverter()
            .ConvertFrom($"M -1 -0.5 L -0.5 -0.5 L 0 0 L -0.5 0.5 L -1 0.5 Z"),
    };

    private Pen borderPen = new Pen(Brushes.Black, 1.0);
    private int horizontalAxisY;
    private int verticalAxisX;

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        if (!HorizontalAxisVisible && !VerticalAxisVisible)
            return;

        borderPen.Thickness = 1.0 / ZoomboxScale;
        handleGeometry.Transform = new ScaleTransform(HandleSize / ZoomboxScale, HandleSize / ZoomboxScale);

        if (HorizontalAxisVisible)
        {
            drawingContext.PushTransform(new TranslateTransform(0, horizontalAxisY));
            drawingContext.DrawGeometry(Brushes.White, borderPen, handleGeometry);
            drawingContext.PushTransform(new RotateTransform(180, ActualWidth / 2, 0));
            drawingContext.DrawGeometry(Brushes.White, borderPen, handleGeometry);
            drawingContext.Pop();
            drawingContext.Pop();
            drawingContext.DrawLine(borderPen, new(0, horizontalAxisY), new(ActualWidth, horizontalAxisY));
        }
        if (VerticalAxisVisible)
        {
            drawingContext.PushTransform(new RotateTransform(90));
            drawingContext.PushTransform(new TranslateTransform(0, -verticalAxisX));
            drawingContext.DrawGeometry(Brushes.White, borderPen, handleGeometry);
            drawingContext.PushTransform(new RotateTransform(180, ActualHeight / 2, 0));
            drawingContext.DrawGeometry(Brushes.White, borderPen, handleGeometry);
            drawingContext.Pop();
            drawingContext.Pop();
            drawingContext.Pop();
            drawingContext.DrawLine(borderPen, new(verticalAxisX, 0), new(verticalAxisX, ActualHeight));
        }
    }

    protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
    {
        // prevent the line from blocking mouse input
        var point = hitTestParameters.HitPoint;
        if (point.X > 0 && point.Y > 0 && point.X < ActualWidth && point.Y < ActualHeight)
            return null;
        return new PointHitTestResult(this, hitTestParameters.HitPoint);
    }

    private SymmetryAxisDirection? IsTouchingHandle(Vector2d position)
    {
        double radius = HandleSize / ZoomboxScale / 2;
        Vector2d left = new(-radius, horizontalAxisY);
        Vector2d right = new(ActualWidth + radius, horizontalAxisY);
        Vector2d up = new(verticalAxisX, -radius);
        Vector2d down = new(verticalAxisX, ActualHeight + radius);

        if (HorizontalAxisVisible && (Math.Abs((left - position).LongestAxis) < radius || Math.Abs((right - position).LongestAxis) < radius))
            return SymmetryAxisDirection.Horizontal;
        if (VerticalAxisVisible && (Math.Abs((up - position).LongestAxis) < radius || Math.Abs((down - position).LongestAxis) < radius))
            return SymmetryAxisDirection.Vertical;
        return null;
    }

    private Vector2d ToVector2d(Point pos) => new Vector2d(pos.X, pos.Y);

    public SymmetryAxisDirection? capturedDirection;

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        var pos = ToVector2d(e.GetPosition(this));
        var dir = IsTouchingHandle(pos);
        if (dir is null)
            return;
        capturedDirection = dir.Value;
        CaptureMouse();
        e.Handled = true;
    }

    private void CallSymmetryDragCommand(SymmetryAxisDirection direction, int position)
    {
        SymmetryAxisDragInfo dragInfo = new(direction, position);
        if (DragCommand is not null && DragCommand.CanExecute(dragInfo))
            DragCommand.Execute(dragInfo);
    }
    private void CallSymmetryDragEndCommand(SymmetryAxisDirection direction)
    {
        if (DragEndCommand is not null && DragEndCommand.CanExecute(direction))
            DragEndCommand.Execute(direction);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        if (capturedDirection is null)
            return;
        ReleaseMouseCapture();

        CallSymmetryDragEndCommand((SymmetryAxisDirection)capturedDirection);

        capturedDirection = null;
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (capturedDirection is null)
            return;
        var pos = ToVector2d(e.GetPosition(this));
        if (capturedDirection == SymmetryAxisDirection.Horizontal)
        {
            horizontalAxisY = (int)Math.Round(Math.Clamp(pos.Y, 0, ActualHeight));
            CallSymmetryDragCommand((SymmetryAxisDirection)capturedDirection, horizontalAxisY);
        }
        else if (capturedDirection == SymmetryAxisDirection.Vertical)
        {
            verticalAxisX = (int)Math.Round(Math.Clamp(pos.X, 0, ActualWidth));
            CallSymmetryDragCommand((SymmetryAxisDirection)capturedDirection, verticalAxisX);
        }
        e.Handled = true;
    }

    private static void OnPositionUpdate(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        var self = (SymmetryOverlay)obj;
        self.horizontalAxisY = self.HorizontalAxisY;
        self.verticalAxisX = self.VerticalAxisX;
        self.InvalidateVisual();
    }
}
