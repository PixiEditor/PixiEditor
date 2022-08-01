using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;

namespace PixiEditor.Views.UserControls.BrushShapeOverlay;
#nullable enable
internal class BrushShapeOverlay : Control
{
    public static readonly DependencyProperty ZoomboxScaleProperty =
        DependencyProperty.Register(nameof(ZoomboxScale), typeof(double), typeof(BrushShapeOverlay), new(1.0, OnZoomboxScaleChanged));

    public static readonly DependencyProperty BrushSizeProperty =
        DependencyProperty.Register(nameof(BrushSize), typeof(int), typeof(BrushShapeOverlay),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MouseEventSourceProperty =
        DependencyProperty.Register(nameof(MouseEventSource), typeof(UIElement), typeof(BrushShapeOverlay), new(null));

    public static readonly DependencyProperty MouseReferenceProperty =
        DependencyProperty.Register(nameof(MouseReference), typeof(UIElement), typeof(BrushShapeOverlay), new(null));

    public UIElement? MouseReference
    {
        get => (UIElement?)GetValue(MouseReferenceProperty);
        set => SetValue(MouseReferenceProperty, value);
    }

    public UIElement? MouseEventSource
    {
        get => (UIElement?)GetValue(MouseEventSourceProperty);
        set => SetValue(MouseEventSourceProperty, value);
    }

    public int BrushSize
    {
        get => (int)GetValue(BrushSizeProperty);
        set => SetValue(BrushSizeProperty, value);
    }

    public double ZoomboxScale
    {
        get => (double)GetValue(ZoomboxScaleProperty);
        set => SetValue(ZoomboxScaleProperty, value);
    }

    private Pen whitePen = new Pen(Brushes.White, 1);
    private Pen blackPen = new Pen(Brushes.Black, 1);
    private Point lastMousePos = new();

    public BrushShapeOverlay()
    {
        Loaded += ControlLoaded;
        Unloaded += ControlUnloaded;
    }

    private void ControlUnloaded(object sender, RoutedEventArgs e)
    {
        if (MouseEventSource is null)
            return;
        MouseEventSource.MouseMove -= SourceMouseMove;
    }

    private void ControlLoaded(object sender, RoutedEventArgs e)
    {
        if (MouseEventSource is null)
            return;
        MouseEventSource.MouseMove += SourceMouseMove;
    }

    private void SourceMouseMove(object sender, MouseEventArgs args)
    {
        if (MouseReference is null)
            return;
        lastMousePos = args.GetPosition(MouseReference);
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var winRect = new Rect(
            (Point)(new Point(Math.Floor(lastMousePos.X), Math.Floor(lastMousePos.Y)) - new Point(BrushSize / 2, BrushSize / 2)),
            new Size(BrushSize, BrushSize)
            );
        var rectI = new RectI((int)winRect.X, (int)winRect.Y, (int)winRect.Width, (int)winRect.Height);

        if (BrushSize < 3)
        {
            drawingContext.DrawRectangle(null, blackPen, winRect);
        }
        else
        {
            var geometry = ConstructEllipseOutline(rectI);
            drawingContext.DrawGeometry(null, whitePen, geometry);
        }
        //drawingContext.DrawRectangle(null, whitePen, winRect.inf);
    }

    private static PathGeometry ConstructEllipseOutline(RectI rectangle)
    {
        var points = EllipseHelper.GenerateEllipseFromRect(rectangle);
        var center = rectangle.Center;
        points.Sort((vec, vec2) => Math.Sign((vec - center).Angle - (vec2 - center).Angle));
        PathFigure figure = new PathFigure()
        {
            StartPoint = new Point(points[0].X, points[0].Y),
            Segments = new PathSegmentCollection(points.Select(static point => new LineSegment(new Point(point.X, point.Y), true))),
            IsClosed = true
        };

        var geometry = new PathGeometry(new PathFigure[] { figure });
        return geometry;
    }

    private static void OnZoomboxScaleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        var self = (BrushShapeOverlay)obj;
        double newScale = (double)args.NewValue;
        self.whitePen.Thickness = 1.0 / newScale;
        self.blackPen.Thickness = 1.0 / newScale;
    }
}
