using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace PixiEditor.AvaloniaUI.Views.Dialogs.Debugging;

public class PointerDebugField : Control
{
    private List<PointerPoint> _lastPoints = new();
    private Pen pen = new(Brushes.Aqua);

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
        int xCount = (int)(DesiredSize.Width / 15);
        int yCount = (int)(DesiredSize.Height / 15);
        for (int y = 0; y < yCount; y++)
        {
            for (int x = 0; x < xCount; x++)
            {
                context.DrawEllipse(PointBrush, null, new Point(x * 15 + 15, y * 15 + 15), 2, 2);
            }
        }
        
        for (var i = 0; i < _lastPoints.Count - 1; i++)
        {
            var point1 = _lastPoints[i];
            var point2 = _lastPoints[i + 1];
            
            context.DrawLine(pen, point1.Position, point2.Position);
        }
    }
}
