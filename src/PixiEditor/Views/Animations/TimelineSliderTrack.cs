using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace PixiEditor.Views.Animations;

internal class TimelineSliderTrack : Track
{
    // Define a dependency property for your scale factor
    public static readonly StyledProperty<double> ScaleFactorProperty =
        AvaloniaProperty.Register<TimelineSliderTrack, double>(nameof(ScaleFactor), defaultValue: 1.0);

    public static readonly StyledProperty<Vector> OffsetProperty = AvaloniaProperty.Register<TimelineSliderTrack, Vector>(
        "Offset");

    public Vector Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    public double ScaleFactor
    {
        get => GetValue(ScaleFactorProperty);
        set => SetValue(ScaleFactorProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(Track);

    static TimelineSliderTrack()
    {
        AffectsArrange<TimelineSliderTrack>(ScaleFactorProperty, OffsetProperty, MinimumProperty);
    }

    // Override the ArrangeOverride method
    protected override Size ArrangeOverride(Size finalSize)
    {
        base.ArrangeOverride(finalSize);
        if (Thumb != null)
        {
            double scaledValue = (Value - Minimum) * ScaleFactor;
            double thumbLength = Orientation == Orientation.Horizontal ? Thumb.DesiredSize.Width : Thumb.DesiredSize.Height;
            
            double thumbPosition = scaledValue - Offset.X;

            Thumb.Arrange(new Rect(thumbPosition, 0, thumbLength, finalSize.Height));
            IncreaseButton?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
        }

        return finalSize;
    }

}
