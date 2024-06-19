using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace PixiEditor.AvaloniaUI.Views.Animations;

public class TimelineTickBar : Control
{
    private const int MinLargeTickDistance = 25;

    public static readonly StyledProperty<IBrush> FillProperty = AvaloniaProperty.Register<TimelineTickBar, IBrush>(
        nameof(Fill), Brushes.Black);

    public static readonly StyledProperty<double> ScaleProperty = AvaloniaProperty.Register<TimelineTickBar, double>(
        "Scale");

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

    static TimelineTickBar()
    {
        AffectsRender<TimelineTickBar>(ScaleProperty, FillProperty);
    }
    
    private readonly int[] possibleLargeTickIntervals = { 1, 5, 10, 50, 100 };

    public override void Render(DrawingContext context)
    {
        double width = Bounds.Width;
        double height = Bounds.Height;

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

        Pen largeTickPen = new Pen(Fill);
        Pen smallTickPen = new Pen(Fill, 0.5);
        
        int max = (int)Math.Ceiling(width / frameWidth);
        
        for (int i = 0; i <= max; i += largeTickInterval)
        {
            double x = i * frameWidth;
            context.DrawLine(largeTickPen, new Point(x, height), new Point(x, height * 0.55f));
            var text = new FormattedText(i.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                Typeface.Default, 12, Fill);
            
            double textCenter = text.WidthIncludingTrailingWhitespace / 2;
            Point textPosition = new Point(x - textCenter, height * 0.05);
            
            context.DrawText(text, textPosition);
        }

        // Draw small ticks
        for (int i = 0; i <= max; i += smallTickInterval)
        {
            if (i % largeTickInterval == 0)
                continue;

            double x = i * frameWidth;
            context.DrawLine(smallTickPen, new Point(x, height), new Point(x, height * 0.7f));
        }
    }
}
