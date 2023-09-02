using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using ChunkyImageLib.Operations;
using PixiEditor.AvaloniaUI.Models.Controllers.InputDevice;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Overlays.BrushShapeOverlay;
#nullable enable
internal class BrushShapeOverlay : Control
{
    public static readonly StyledProperty<double> ZoomboxScaleProperty =
        AvaloniaProperty.Register<BrushShapeOverlay, double>(nameof(ZoomboxScale), defaultValue: 1.0);

    public static readonly StyledProperty<int> BrushSizeProperty =
        AvaloniaProperty.Register<BrushShapeOverlay, int>(nameof(BrushSize), defaultValue: 1);

    public static readonly StyledProperty<InputElement?> MouseEventSourceProperty =
        AvaloniaProperty.Register<BrushShapeOverlay, InputElement?>(nameof(MouseEventSource), defaultValue: null);

    public static readonly StyledProperty<InputElement?> MouseReferenceProperty =
        AvaloniaProperty.Register<BrushShapeOverlay, InputElement?>(nameof(MouseReference), defaultValue: null);

    public static readonly StyledProperty<BrushShape> BrushShapeProperty =
        AvaloniaProperty.Register<BrushShapeOverlay, BrushShape>(nameof(BrushShape), defaultValue: BrushShape.Circle);

    public BrushShape BrushShape
    {
        get => (BrushShape)GetValue(BrushShapeProperty);
        set => SetValue(BrushShapeProperty, value);
    }

    public InputElement? MouseReference
    {
        get => (InputElement?)GetValue(MouseReferenceProperty);
        set => SetValue(MouseReferenceProperty, value);
    }

    public InputElement? MouseEventSource
    {
        get => (InputElement?)GetValue(MouseEventSourceProperty);
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

    private MouseUpdateController mouseUpdateController;

    static BrushShapeOverlay()
    {
        AffectsRender<BrushShapeOverlay>(BrushShapeProperty);
        AffectsRender<BrushShapeOverlay>(BrushSizeProperty);

        ZoomboxScaleProperty.Changed.Subscribe(OnZoomboxScaleChanged);
    }

    public BrushShapeOverlay()
    {
        Loaded += ControlLoaded;
        Unloaded += ControlUnloaded;
    }

    private void ControlUnloaded(object sender, RoutedEventArgs e)
    {
        if (MouseEventSource is null)
            return;
        
        mouseUpdateController.Dispose();
    }

    private void ControlLoaded(object sender, RoutedEventArgs e)
    {
        if (MouseEventSource is null)
            return;
        
        mouseUpdateController = new MouseUpdateController(MouseEventSource, SourceMouseMove);
    }

    private void SourceMouseMove(PointerEventArgs args)
    {
        if (MouseReference is null || BrushShape == BrushShape.Hidden)
            return;
        lastMousePos = args.GetPosition(MouseReference);
        InvalidateVisual();
    }

    public override void Render(DrawingContext drawingContext)
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
                Segments = new PathSegments()
                {
                    new LineSegment { Point = new Point(lp.X, lp.Y - 1) },
                    new LineSegment { Point = new Point(lp.X + 1, lp.Y - 1) },
                    new LineSegment { Point = new Point(lp.X + 1, lp.Y) },
                    new LineSegment { Point = new Point(lp.X + 2, lp.Y) },
                    new LineSegment { Point = new Point(lp.X + 2, lp.Y + 1) },
                    new LineSegment { Point = new Point(lp.X + 2, lp.Y + 1) },
                    new LineSegment { Point = new Point(lp.X + 1, lp.Y + 1) },
                    new LineSegment { Point = new Point(lp.X + 1, lp.Y + 2) },
                    new LineSegment { Point = new Point(lp.X, lp.Y + 2) },
                    new LineSegment { Point = new Point(lp.X, lp.Y + 1) },
                    new LineSegment { Point = new Point(lp.X - 1, lp.Y + 1) },
                    new LineSegment { Point = new Point(lp.X - 1, lp.Y) }
                },
                IsClosed = true
            };

            var geometry = new PathGeometry() { Figures = new PathFigures() { figure } };
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

        PathSegments segments = new();
        segments.AddRange(finalPoints.Select(static point => new LineSegment { Point = new(point.X, point.Y) }));

        PathFigure figure = new PathFigure()
        {
            StartPoint = new Point(finalPoints[0].X, finalPoints[0].Y),
            Segments = segments,
            IsClosed = true
        };

        var geometry = new PathGeometry() { Figures = new PathFigures() { figure }};
        return geometry;
    }

    private static void OnZoomboxScaleChanged(AvaloniaPropertyChangedEventArgs<double> e)
    {
        var self = (BrushShapeOverlay)e.Sender;
        double newScale = e.NewValue.Value;
        self.whitePen.Thickness = 1.0 / newScale;
    }
}
