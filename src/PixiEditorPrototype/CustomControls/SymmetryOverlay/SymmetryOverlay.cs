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
    public static readonly DependencyProperty HorizontalPositionProperty =
        DependencyProperty.Register(nameof(HorizontalPosition), typeof(int), typeof(SymmetryOverlay),
            new(0, OnPositionUpdate));

    public int HorizontalPosition
    {
        get => (int)GetValue(HorizontalPositionProperty);
        set => SetValue(HorizontalPositionProperty, value);
    }

    public static readonly DependencyProperty VerticalPositionProperty =
        DependencyProperty.Register(nameof(VerticalPosition), typeof(int), typeof(SymmetryOverlay),
            new(0, OnPositionUpdate));

    public int VerticalPosition
    {
        get => (int)GetValue(VerticalPositionProperty);
        set => SetValue(VerticalPositionProperty, value);
    }

    public static readonly DependencyProperty HorizontalVisibleProperty =
        DependencyProperty.Register(nameof(HorizontalVisible), typeof(bool), typeof(SymmetryOverlay),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public bool HorizontalVisible
    {
        get => (bool)GetValue(HorizontalVisibleProperty);
        set => SetValue(HorizontalVisibleProperty, value);
    }

    public static readonly DependencyProperty VerticalVisibleProperty =
        DependencyProperty.Register(nameof(VerticalVisible), typeof(bool), typeof(SymmetryOverlay),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public bool VerticalVisible
    {
        get => (bool)GetValue(VerticalVisibleProperty);
        set => SetValue(VerticalVisibleProperty, value);
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
    private int horizontalPosition;
    private int verticalPosition;

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        if (!HorizontalVisible && !VerticalVisible)
            return;

        borderPen.Thickness = 1.0 / ZoomboxScale;
        handleGeometry.Transform = new ScaleTransform(HandleSize / ZoomboxScale, HandleSize / ZoomboxScale);

        if (HorizontalVisible)
        {
            drawingContext.PushTransform(new TranslateTransform(0, horizontalPosition));
            drawingContext.DrawGeometry(Brushes.White, borderPen, handleGeometry);
            drawingContext.PushTransform(new RotateTransform(180, ActualWidth / 2, 0));
            drawingContext.DrawGeometry(Brushes.White, borderPen, handleGeometry);
            drawingContext.Pop();
            drawingContext.Pop();
            drawingContext.DrawLine(borderPen, new(0, horizontalPosition), new(ActualWidth, horizontalPosition));
        }
        if (VerticalVisible)
        {
            drawingContext.PushTransform(new RotateTransform(90));
            drawingContext.PushTransform(new TranslateTransform(0, -verticalPosition));
            drawingContext.DrawGeometry(Brushes.White, borderPen, handleGeometry);
            drawingContext.PushTransform(new RotateTransform(180, ActualHeight / 2, 0));
            drawingContext.DrawGeometry(Brushes.White, borderPen, handleGeometry);
            drawingContext.Pop();
            drawingContext.Pop();
            drawingContext.Pop();
            drawingContext.DrawLine(borderPen, new(verticalPosition, 0), new(verticalPosition, ActualHeight));
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

    private SymmetryDirection? IsTouchingHandle(Vector2d position)
    {
        double radius = HandleSize / ZoomboxScale / 2;
        Vector2d left = new(-radius, horizontalPosition);
        Vector2d right = new(ActualWidth + radius, horizontalPosition);
        Vector2d up = new(verticalPosition, -radius);
        Vector2d down = new(verticalPosition, ActualHeight + radius);

        if (HorizontalVisible && ((left - position).Length < radius || (right - position).Length < radius))
            return SymmetryDirection.Horizontal;
        if (VerticalVisible && ((up - position).Length < radius || (down - position).Length < radius))
            return SymmetryDirection.Vertical;
        return null;
    }

    private Vector2d ToVector2d(Point pos) => new Vector2d(pos.X, pos.Y);

    public SymmetryDirection? capturedDirection;

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

    private void CallSymmetryDragCommand(SymmetryDirection direction, int position)
    {
        SymmetryDragInfo dragInfo = new(direction, position);
        if (DragCommand is not null && DragCommand.CanExecute(dragInfo))
            DragCommand.Execute(dragInfo);
    }
    private void CallSymmetryDragEndCommand(SymmetryDirection direction)
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

        CallSymmetryDragEndCommand((SymmetryDirection)capturedDirection);

        capturedDirection = null;
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (capturedDirection is null)
            return;
        var pos = ToVector2d(e.GetPosition(this));
        if (capturedDirection == SymmetryDirection.Horizontal)
        {
            horizontalPosition = (int)Math.Round(Math.Clamp(pos.Y, 0, ActualHeight));
            CallSymmetryDragCommand((SymmetryDirection)capturedDirection, horizontalPosition);
        }
        else if (capturedDirection == SymmetryDirection.Vertical)
        {
            verticalPosition = (int)Math.Round(Math.Clamp(pos.X, 0, ActualWidth));
            CallSymmetryDragCommand((SymmetryDirection)capturedDirection, verticalPosition);
        }
        e.Handled = true;
    }

    private static void OnPositionUpdate(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        var self = (SymmetryOverlay)obj;
        self.horizontalPosition = self.HorizontalPosition;
        self.verticalPosition = self.VerticalPosition;
        self.InvalidateVisual();
    }
}
