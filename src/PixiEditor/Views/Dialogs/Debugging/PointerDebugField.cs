using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace PixiEditor.Views.Dialogs.Debugging;

public class PointerDebugField : Control
{
    private List<PointerPoint> _lastPoints = new();
    
    private Pen normalPen = new(Brushes.Aqua);
    private Pen redPen = new(Brushes.Red);
    private Pen yellowPen = new(Brushes.Yellow);
    private Pen greenPen = new(Brushes.Lime);

    public static readonly StyledProperty<int> PointCountProperty =
        AvaloniaProperty.Register<PointerDebugPopup, int>(nameof(PointCount));
    
    public int PointCount
    {
        get => GetValue(PointCountProperty);
        set => SetValue(PointCountProperty, value);
    }

    public static readonly StyledProperty<IBrush> PointBrushProperty =
        AvaloniaProperty.Register<PointerDebugPopup, IBrush>(nameof(PointBrush));

    public IBrush PointBrush
    {
        get => GetValue(PointBrushProperty);
        set => SetValue(PointBrushProperty, value);
    }

    public static readonly StyledProperty<PointerDebugPopup.ScrollStats> ScrollInfoProperty =
        AvaloniaProperty.Register<PointerDebugPopup, PointerDebugPopup.ScrollStats>(nameof(ScrollInfo));

    public PointerDebugPopup.ScrollStats ScrollInfo
    {
        get => GetValue(ScrollInfoProperty);
        set => SetValue(ScrollInfoProperty, value);
    }

    static PointerDebugField()
    {
        ScrollInfoProperty.Changed.AddClassHandler<PointerDebugField>(OnChange);
    }

    private static void OnChange(PointerDebugField sender, AvaloniaPropertyChangedEventArgs args)
    {
        sender.InvalidateVisual();
    }

    public void ReportPoint(PointerPoint point)
    {
        // if (_lastPoints.Count > 5000) _lastPoints.Clear();
        
        _lastPoints.Add(point);
        PointCount = _lastPoints.Count;
        
        InvalidateVisual();
    }

    public void ClearPoints()
    {
        _lastPoints.Clear();
        PointCount = _lastPoints.Count;
        
        InvalidateVisual();
    }

    protected override Size MeasureOverride(Size availableSize) => availableSize;

    public override void Render(DrawingContext context)
    {
        var size = DesiredSize;

        const double pointDistance = 15;
        const double pointRadius = 2;
        const double scrollSensitivity = 15;
        
        var xCount = (int)(size.Width / pointDistance);
        var yCount = (int)(size.Height / pointDistance);

        double xMidpoint = (size.Width - xCount * pointDistance) / 2d + pointDistance;
        double yMidpoint = (size.Height - yCount * pointDistance) / 2d + pointDistance;

        double xScroll = (ScrollInfo.TotalScroll.X % scrollSensitivity) * (pointDistance / scrollSensitivity);
        double yScroll = (ScrollInfo.TotalScroll.Y % scrollSensitivity) * (pointDistance / scrollSensitivity);

        double xOffset = xMidpoint + xScroll;
        double yOffset = yMidpoint + yScroll;

        context.PushClip(new RoundedRect(new Rect(size), 10));
        
        for (int y = -2; y < yCount + 2; y++)
        {
            for (int x = -2; x < xCount + 2; x++)
            {
                var point = new Point(x * pointDistance + xOffset, y * pointDistance + yOffset);
                
                context.DrawEllipse(PointBrush, null, point, pointRadius, pointRadius);
            }
        }
        
        for (var i = 0; i < _lastPoints.Count - 1; i++)
        {
            var point1 = _lastPoints[i];
            var point2 = _lastPoints[i + 1];

            var pen = normalPen;
            
            if (point2.Properties.IsLeftButtonPressed)
            {
                pen = redPen;
            }
            else if (point2.Properties.IsRightButtonPressed)
            {
                pen = greenPen;
            }
            else if (point2.Properties.IsMiddleButtonPressed)
            {
                pen = yellowPen;
            }
            
            context.DrawLine(pen, point1.Position, point2.Position);
        }
    }
}
