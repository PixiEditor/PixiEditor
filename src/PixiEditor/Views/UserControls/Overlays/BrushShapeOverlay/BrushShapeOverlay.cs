using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Controllers;
using PixiEditor.Numerics;
using PixiEditor.Views;
using PixiEditor.Views.UserControls;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;
using Point = System.Windows.Point;

namespace PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;
#nullable enable
internal class BrushShapeOverlay : Control
{
    public static readonly DependencyProperty ZoomboxScaleProperty =
        DependencyProperty.Register(nameof(ZoomboxScale), typeof(double), typeof(BrushShapeOverlay), new(1.0, OnZoomboxScaleChanged));

    public static readonly DependencyProperty BrushSizeProperty =
        DependencyProperty.Register(nameof(BrushSize), typeof(int), typeof(BrushShapeOverlay),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MouseEventSourceProperty =
        DependencyProperty.Register(nameof(MouseEventSource), typeof(FrameworkElement), typeof(BrushShapeOverlay), new(null));

    public static readonly DependencyProperty MouseReferenceProperty =
        DependencyProperty.Register(nameof(MouseReference), typeof(UIElement), typeof(BrushShapeOverlay), new(null));

    public static readonly DependencyProperty BrushShapeProperty =
        DependencyProperty.Register(nameof(BrushShape), typeof(BrushShape), typeof(BrushShapeOverlay),
            new FrameworkPropertyMetadata(BrushShape.Circle, FrameworkPropertyMetadataOptions.AffectsRender));

    public BrushShape BrushShape
    {
        get => (BrushShape)GetValue(BrushShapeProperty);
        set => SetValue(BrushShapeProperty, value);
    }

    public UIElement? MouseReference
    {
        get => (UIElement?)GetValue(MouseReferenceProperty);
        set => SetValue(MouseReferenceProperty, value);
    }

    public FrameworkElement? MouseEventSource
    {
        get => (FrameworkElement?)GetValue(MouseEventSourceProperty);
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

    private Pen whitePen = new Pen(Brushes.LightGray, 1);
    private Point lastMousePos = new();

    private MouseUpdateController? mouseUpdateController;

    public BrushShapeOverlay()
    {
        Loaded += ControlLoaded;
        Unloaded += ControlUnloaded;
    }

    private void ControlUnloaded(object sender, RoutedEventArgs e)
    {
        mouseUpdateController?.Dispose();
    }

    private void ControlLoaded(object sender, RoutedEventArgs e)
    {
        if (MouseEventSource is null)
            return;
        
        mouseUpdateController = new MouseUpdateController(MouseEventSource, SourceMouseMove);
    }

    private void SourceMouseMove(object sender, MouseEventArgs args)
    {
        if (MouseReference is null || BrushShape == BrushShape.Hidden)
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
        switch (BrushShape)
        {
            case BrushShape.Pixel:
                drawingContext.DrawRectangle(
                    null, whitePen, new Rect(new Point(Math.Floor(lastMousePos.X), Math.Floor(lastMousePos.Y)), new Size(1, 1)));
                break;
            case BrushShape.Square:
                drawingContext.DrawRectangle(null, whitePen, winRect);
                break;
            case BrushShape.Circle:
                DrawCircleBrushShape(drawingContext, winRect);
                break;
        }
    }

    private void DrawCircleBrushShape(DrawingContext drawingContext, Rect winRect)
    {
        var rectI = new RectI((int)winRect.X, (int)winRect.Y, (int)winRect.Width, (int)winRect.Height);
        if (BrushSize < 3)
        {
            drawingContext.DrawRectangle(null, whitePen, winRect);
        }
        else if (BrushSize == 3)
        {
            var lp = new VecI((int)lastMousePos.X, (int)lastMousePos.Y);
            PathFigure figure = new PathFigure()
            {
                StartPoint = new Point(lp.X, lp.Y),
                Segments = new PathSegmentCollection()
                {
                    new LineSegment(new(lp.X, lp.Y - 1), true),
                    new LineSegment(new(lp.X + 1, lp.Y - 1), true),
                    new LineSegment(new(lp.X + 1, lp.Y), true),
                    new LineSegment(new(lp.X + 2, lp.Y), true),
                    new LineSegment(new(lp.X + 2, lp.Y + 1), true),
                    new LineSegment(new(lp.X + 2, lp.Y + 1), true),
                    new LineSegment(new(lp.X + 1, lp.Y + 1), true),
                    new LineSegment(new(lp.X + 1, lp.Y + 2), true),
                    new LineSegment(new(lp.X, lp.Y + 2), true),
                    new LineSegment(new(lp.X, lp.Y + 1), true),
                    new LineSegment(new(lp.X - 1, lp.Y + 1), true),
                    new LineSegment(new(lp.X - 1, lp.Y), true),
                },
                IsClosed = true
            };

            var geometry = new PathGeometry(new PathFigure[] { figure });
            drawingContext.DrawGeometry(null, whitePen, geometry);
        }
        else if (BrushSize > 200)
        {
            VecD center = rectI.Center;
            drawingContext.DrawEllipse(null, whitePen, new Point(center.X, center.Y), rectI.Width / 2.0, rectI.Height / 2.0);
        }
        else
        {
            var geometry = ConstructEllipseOutline(rectI);
            drawingContext.DrawGeometry(null, whitePen, geometry);
        }
    }

    private static int Mod(int x, int m) => (x % m + m) % m;

    private static PathGeometry ConstructEllipseOutline(RectI rectangle)
    {
        var center = rectangle.Center;
        var points = EllipseHelper.GenerateEllipseFromRect(rectangle);
        points.Sort((vec, vec2) => Math.Sign((vec - center).Angle - (vec2 - center).Angle));
        List<VecI> finalPoints = new();
        for (int i = 0; i < points.Count; i++)
        {
            VecI prev = points[Mod(i - 1, points.Count)];
            VecI point = points[i];
            VecI next = points[Mod(i + 1, points.Count)];

            bool atBottom = point.Y >= center.Y;
            bool onRight = point.X >= center.X;
            if (atBottom)
            {
                if (onRight)
                {
                    if (prev.Y != point.Y)
                        finalPoints.Add(new(point.X + 1, point.Y));
                    finalPoints.Add(new(point.X + 1, point.Y + 1));
                    if (next.X != point.X)
                        finalPoints.Add(new(point.X, point.Y + 1));

                }
                else
                {
                    if (prev.X != point.X)
                        finalPoints.Add(new(point.X + 1, point.Y + 1));
                    finalPoints.Add(new(point.X, point.Y + 1));
                    if (next.Y != point.Y)
                        finalPoints.Add(point);
                }
            }
            else
            {
                if (onRight)
                {
                    if (prev.X != point.X)
                        finalPoints.Add(point);
                    finalPoints.Add(new(point.X + 1, point.Y));
                    if (next.Y != point.Y)
                        finalPoints.Add(new(point.X + 1, point.Y + 1));
                }
                else
                {
                    if (prev.Y != point.Y)
                        finalPoints.Add(new(point.X, point.Y + 1));
                    finalPoints.Add(point);
                    if (next.X != point.X)
                        finalPoints.Add(new(point.X + 1, point.Y));
                }
            }
        }

        PathFigure figure = new PathFigure()
        {
            StartPoint = new Point(finalPoints[0].X, finalPoints[0].Y),
            Segments = new PathSegmentCollection(finalPoints.Select(static point => new LineSegment(new Point(point.X, point.Y), true))),
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
    }
}
