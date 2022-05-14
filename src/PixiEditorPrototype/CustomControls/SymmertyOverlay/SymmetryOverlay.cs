using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.CustomControls.SymmertyOverlay;

internal class SymmetryOverlay : Control
{
    public static readonly DependencyProperty HorizontalPositionProperty =
        DependencyProperty.Register(nameof(HorizontalPosition), typeof(int), typeof(SymmetryOverlay),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    public int HorizontalPosition
    {
        get => (int)GetValue(HorizontalPositionProperty);
        set => SetValue(HorizontalPositionProperty, value);
    }

    public static readonly DependencyProperty VerticalPositionProperty =
        DependencyProperty.Register(nameof(VerticalPosition), typeof(int), typeof(SymmetryOverlay),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

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

    private const double HandleSize = 16;
    private Pen borderPen = new Pen(Brushes.Black, 1.0);

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        if (!HorizontalVisible && !VerticalVisible)
            return;

        borderPen.Thickness = 1.0 / ZoomboxScale;
        double radius = HandleSize / ZoomboxScale / 2;

        if (HorizontalVisible)
        {
            drawingContext.DrawEllipse(Brushes.White, borderPen, new(-radius, HorizontalPosition), radius, radius);
            drawingContext.DrawEllipse(Brushes.White, borderPen, new(ActualWidth + radius, HorizontalPosition), radius, radius);
            drawingContext.DrawLine(borderPen, new(0, HorizontalPosition), new(ActualWidth, HorizontalPosition));
        }
        if (VerticalVisible)
        {
            drawingContext.DrawEllipse(Brushes.White, borderPen, new(VerticalPosition, -radius), radius, radius);
            drawingContext.DrawEllipse(Brushes.White, borderPen, new(VerticalPosition, ActualHeight + radius), radius, radius);
            drawingContext.DrawLine(borderPen, new(VerticalPosition, 0), new(VerticalPosition, ActualHeight));
        }
    }

    protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
    {
        // prevent the line from blocking mouse input
        if (IsTouchingHandle(ToVector2d(hitTestParameters.HitPoint)) is not null)
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        return null;
    }

    private SymmetryDirection? IsTouchingHandle(Vector2d position)
    {
        double radius = HandleSize / ZoomboxScale / 2;
        Vector2d left = new(-radius, HorizontalPosition);
        Vector2d right = new(ActualWidth + radius, HorizontalPosition);
        Vector2d up = new(VerticalPosition, -radius);
        Vector2d down = new(VerticalPosition, ActualHeight + radius);

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
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        if (capturedDirection is null)
            return;
        capturedDirection = null;
        ReleaseMouseCapture();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (capturedDirection is null)
            return;
        var pos = ToVector2d(e.GetPosition(this));
        if (capturedDirection == SymmetryDirection.Horizontal)
            HorizontalPosition = (int)Math.Round(Math.Clamp(pos.Y, 0, ActualHeight));
        if (capturedDirection == SymmetryDirection.Vertical)
            VerticalPosition = (int)Math.Round(Math.Clamp(pos.X, 0, ActualWidth));
    }
}
