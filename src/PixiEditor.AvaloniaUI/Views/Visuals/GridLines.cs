using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PixiEditor.AvaloniaUI.Helpers.Converters;
using PixiEditor.AvaloniaUI.Views.Overlays;

namespace PixiEditor.AvaloniaUI.Views.Visuals;

public class GridLines : Overlay
{
    public static readonly StyledProperty<int> RowsProperty = AvaloniaProperty.Register<GridLines, int>(
        nameof(Rows));

    public static readonly StyledProperty<int> ColumnsProperty = AvaloniaProperty.Register<GridLines, int>(
        nameof(Columns));

    public static readonly StyledProperty<int> PixelWidthProperty = AvaloniaProperty.Register<GridLines, int>(
        nameof(PixelWidth));

    public static readonly StyledProperty<int> PixelHeightProperty = AvaloniaProperty.Register<GridLines, int>(
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

    private const double PenWidth = 0.8d;
    private Pen pen1 = new(Brushes.Black, PenWidth);
    private Pen pen2 = new(Brushes.White, PenWidth);
    private ThresholdVisibilityConverter visibilityConverter = new(){ Threshold = 10 };

    static GridLines()
    {
        IsVisibleProperty.Changed.Subscribe(OnIsVisibleChanged);
    }

    public GridLines()
    {
        IsHitTestVisible = false;
    }

    protected override void ZoomChanged(double newZoom)
    {
        IsVisible = IsVisible && visibilityConverter.Check(newZoom);
    }

    public override void Render(DrawingContext context)
    {
        // Draw lines in vertical and horizontal directions, size should be relative to the scale

        base.Render(context);
        double width = PixelWidth;
        double height = PixelHeight;

        double columnWidth = width / Columns;
        double rowHeight = height / Rows;

        pen1.Thickness = ReciprocalConverter.Convert(ZoomScale);
        pen2.Thickness = ReciprocalConverter.Convert(ZoomScale, 1.2);

        for (int i = 0; i < Columns; i++)
        {
            double x = i * columnWidth;
            context.DrawLine(pen1, new Point(x, 0), new Point(x, height));
            context.DrawLine(pen2, new Point(x, 0), new Point(x, height));
        }

        for (int i = 0; i < Rows; i++)
        {
            double y = i * rowHeight;
            context.DrawLine(pen1, new Point(0, y), new Point(width, y));
            context.DrawLine(pen2, new Point(0, y), new Point(width, y));
        }
    }

    private static void OnIsVisibleChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is GridLines gridLines)
        {
            gridLines.Refresh();
        }
    }
}
