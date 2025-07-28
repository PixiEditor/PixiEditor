using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace PixiEditor.Views.Animations;

public class TimelineTickBar : Control
{
    private const int MinLargeTickDistance = 25;

    public static readonly StyledProperty<IBrush> FillProperty = AvaloniaProperty.Register<TimelineTickBar, IBrush>(
        nameof(Fill), Brushes.Black);

    public static readonly StyledProperty<double> ScaleProperty = AvaloniaProperty.Register<TimelineTickBar, double>(
        "Scale");
    
    public static readonly StyledProperty<Vector> OffsetProperty = AvaloniaProperty.Register<TimelineTickBar, Vector>("Offset");

    public static readonly StyledProperty<int> MinValueProperty = AvaloniaProperty.Register<TimelineTickBar, int>(
        nameof(MinValue), 1);

    public static readonly StyledProperty<IBrush> ForegroundProperty = AvaloniaProperty.Register<TimelineTickBar, IBrush>(
        nameof(Foreground), Brushes.White);

    public IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }
    
    public int MinValue
    {
        get => GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }

    public double Scale
    {
        get => GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    public IBrush Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public Vector Offset
    {
        get { return (Vector)GetValue(OffsetProperty); }
        set { SetValue(OffsetProperty, value); }
    }

    public double MinLeftOffset
    {
        get { return (double)GetValue(MinLeftOffsetProperty); }
        set { SetValue(MinLeftOffsetProperty, value); }
    }

    static TimelineTickBar()
    {
        AffectsRender<TimelineTickBar>(ScaleProperty, FillProperty, OffsetProperty, MinValueProperty, ForegroundProperty, MinLeftOffsetProperty);
    }
    
    private readonly int[] possibleLargeTickIntervals = { 1, 5, 10, 50, 100 };
    public static readonly StyledProperty<double> MinLeftOffsetProperty = AvaloniaProperty.Register<TimelineTickBar, double>("MinOffset");

    public override void Render(DrawingContext context)
    {
        double height = Bounds.Height;
        
        int visibleMin = (int)Math.Floor(Offset.X / Scale);
        int visibleMax = (int)Math.Ceiling((Offset.X + Bounds.Width) / Scale);

        double frameWidth = Scale;
        int largeTickInterval = possibleLargeTickIntervals[0];
        
        foreach (int interval in possibleLargeTickIntervals)
        {
            if (interval * frameWidth >= MinLargeTickDistance)
            {
                largeTickInterval = interval;
                break;
            }
        }
        
        int smallTickInterval = largeTickInterval / 5;
        if (smallTickInterval < 1)
        {
            smallTickInterval = 1;
        }

        Pen largeTickPen = new Pen(Fill, thickness: 2);
        Pen smallTickPen = new Pen(Fill, 1.5);
        
        int largeStart = visibleMin - (visibleMin % largeTickInterval) - MinValue;
        
        RenderBigTicks(context, largeStart, visibleMax, largeTickInterval, frameWidth, largeTickPen, height);
        
        int smallStart = visibleMin - (visibleMin % smallTickInterval);
        
        RenderMinTicks(context, smallStart, visibleMax, smallTickInterval, largeTickInterval, frameWidth, smallTickPen, height);
    }

    private void RenderMinTicks(DrawingContext context, int smallStart, int visibleMax, int smallTickInterval,
        int largeTickInterval, double frameWidth, Pen smallTickPen, double height)
    {
        for (int i = smallStart; i <= visibleMax; i += smallTickInterval)
        {
            if (i % largeTickInterval == 0)
                continue;

            double x = i * frameWidth - Offset.X + MinLeftOffset;
            context.DrawLine(smallTickPen, new Point(x, height), new Point(x, height * 0.7f));
        }
    }

    private void RenderBigTicks(DrawingContext context, int largeStart, int visibleMax, int largeTickInterval,
        double frameWidth, Pen largeTickPen, double height)
    {
        for (int i = largeStart; i <= visibleMax; i += largeTickInterval)
        {
            double x = i * frameWidth - Offset.X + MinLeftOffset;
            context.DrawLine(largeTickPen, new Point(x, height), new Point(x, height * 0.55f));
            
            var text = new FormattedText((i + MinValue).ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                Typeface.Default, 12, Foreground);
            
            double textCenter = text.WidthIncludingTrailingWhitespace / 2;
            Point textPosition = new Point(x - textCenter, height * 0.05);
            
            context.DrawText(text, textPosition);
        }
    }
}
