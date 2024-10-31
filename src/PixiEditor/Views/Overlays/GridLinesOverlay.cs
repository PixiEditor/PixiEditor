using Avalonia;
using Avalonia.Media;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.Helpers.Converters;
using Drawie.Numerics;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;
using Point = Avalonia.Point;

namespace PixiEditor.Views.Overlays;

public class GridLinesOverlay : Overlay
{
    public static readonly StyledProperty<int> RowsProperty = AvaloniaProperty.Register<GridLinesOverlay, int>(
        nameof(Rows));

    public static readonly StyledProperty<int> ColumnsProperty = AvaloniaProperty.Register<GridLinesOverlay, int>(
        nameof(Columns));

    public static readonly StyledProperty<int> PixelWidthProperty = AvaloniaProperty.Register<GridLinesOverlay, int>(
        nameof(PixelWidth));

    public static readonly StyledProperty<int> PixelHeightProperty = AvaloniaProperty.Register<GridLinesOverlay, int>(
        nameof(PixelHeight));

    public int PixelHeight
    {
        get => GetValue(PixelHeightProperty);
        set => SetValue(PixelHeightProperty, value);
    }

    public int PixelWidth
    {
        get => GetValue(PixelWidthProperty);
        set => SetValue(PixelWidthProperty, value);
    }

    public int Columns
    {
        get => GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public int Rows
    {
        get => GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    private const float PenWidth = 0.8f;
    private Paint pen1 = new Paint() { Color = Colors.Black, StrokeWidth = PenWidth, IsAntiAliased = true, Style = PaintStyle.Stroke };
    private Paint pen2 = new Paint() { Color = Colors.White, StrokeWidth = PenWidth, IsAntiAliased = true, Style = PaintStyle.Stroke };
    private ThresholdVisibilityConverter visibilityConverter = new(){ Threshold = 10 };

    static GridLinesOverlay()
    {
        IsVisibleProperty.Changed.Subscribe(OnIsVisibleChanged);
    }

    public GridLinesOverlay()
    {
        IsHitTestVisible = false;
    }

    public override bool CanRender()
    {
        return visibilityConverter.Check(ZoomScale);
    }

    public override void RenderOverlay(Canvas context, RectD canvasBounds)
    {
        // Draw lines in vertical and horizontal directions, size should be relative to the scale

        double width = PixelWidth;
        double height = PixelHeight;

        double columnWidth = width / Columns;
        double rowHeight = height / Rows;

        pen1.StrokeWidth = (float)ReciprocalConverter.Convert(ZoomScale);
        pen2.StrokeWidth = (float)ReciprocalConverter.Convert(ZoomScale, 1.2);

        for (int i = 0; i < Columns; i++)
        {
            double x = i * columnWidth;
            context.DrawLine(new VecD(x, 0), new VecD(x, height), pen1);
            context.DrawLine(new VecD(x, 0), new VecD(x, height), pen2);
        }

        for (int i = 0; i < Rows; i++)
        {
            double y = i * rowHeight;
            context.DrawLine(new VecD(0, y), new VecD(width, y), pen1);
            context.DrawLine(new VecD(0, y), new VecD(width, y), pen2);
        }
    }

    private static void OnIsVisibleChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is GridLinesOverlay gridLines)
        {
            gridLines.Refresh();
        }
    }
}
