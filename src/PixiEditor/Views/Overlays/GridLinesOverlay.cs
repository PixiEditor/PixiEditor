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
    public static readonly StyledProperty<double> GridXSizeProperty = AvaloniaProperty.Register<GridLinesOverlay, double>(
        nameof(GridXSize));

    public static readonly StyledProperty<double> GridYSizeProperty = AvaloniaProperty.Register<GridLinesOverlay, double>(
        nameof(GridYSize));

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

    public double GridXSize
    {
        get => GetValue(GridXSizeProperty);
        set => SetValue(GridXSizeProperty, value);
    }

    public double GridYSize
    {
        get => GetValue(GridYSizeProperty);
        set => SetValue(GridYSizeProperty, value);
    }

    private const float PenWidth = 0.8f;
    private Paint pen1 = new Paint() { Color = Colors.Black, StrokeWidth = PenWidth, IsAntiAliased = true, Style = PaintStyle.Stroke };
    private Paint pen2 = new Paint() { Color = Colors.White, StrokeWidth = PenWidth, IsAntiAliased = true, Style = PaintStyle.Stroke };
    private ThresholdVisibilityConverter visibilityConverter = new(){ Threshold = 10 };

    static GridLinesOverlay()
    {
        IsVisibleProperty.Changed.Subscribe(OnIsVisibleChanged);
        GridXSizeProperty.Changed.Subscribe(OnNumberChanged);
        GridYSizeProperty.Changed.Subscribe(OnNumberChanged);
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

        double columnWidth = GridXSize;
        double rowHeight = GridYSize;

        pen1.StrokeWidth = (float)ReciprocalConverter.Convert(ZoomScale);
        pen2.StrokeWidth = (float)ReciprocalConverter.Convert(ZoomScale, 1.2);

        for (int i = 0; i < width / columnWidth; i++)
        {
            double x = i * columnWidth;
            context.DrawLine(new VecD(x, 0), new VecD(x, height), pen1);
            context.DrawLine(new VecD(x, 0), new VecD(x, height), pen2);
        }

        for (int i = 0; i < height / rowHeight; i++)
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
    private static void OnNumberChanged(AvaloniaPropertyChangedEventArgs<double> e)
    {
        if (e.Sender is GridLinesOverlay gridLines)
        {
            gridLines.Refresh();
        }
    }
}
