using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace PixiEditor.AvaloniaUI.Views.Animations;

internal class TimelineSliderTrack : Track
{
    // Define a dependency property for your scale factor
    public static readonly StyledProperty<double> ScaleFactorProperty =
        AvaloniaProperty.Register<TimelineSliderTrack, double>(nameof(ScaleFactor), defaultValue: 1.0);

    public double ScaleFactor
    {
        get => GetValue(ScaleFactorProperty);
        set => SetValue(ScaleFactorProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(Track);

    // Override the ArrangeOverride method
    protected override Size ArrangeOverride(Size finalSize)
    {
        base.ArrangeOverride(finalSize);
        if (Thumb != null)
        {
            double scaledValue = Value * ScaleFactor;
            double thumbLength = Orientation == Orientation.Horizontal ? Thumb.DesiredSize.Width : Thumb.DesiredSize.Height;
            
            double thumbPosition = scaledValue;

            Thumb.Arrange(new Rect(thumbPosition, 0, thumbLength, finalSize.Height));
            
            Rect decreaseButtonRect = new Rect(0, 0, thumbPosition, finalSize.Height);
            Rect increaseButtonRect = new Rect(thumbPosition + thumbLength, 0, finalSize.Width - thumbPosition - thumbLength, finalSize.Height);
            if (decreaseButtonRect.Width < 0)
            {
                decreaseButtonRect = new Rect(0, 0, 0, 0);
            }
            
            if (increaseButtonRect.Width < 0)
            {
                increaseButtonRect = new Rect(0, 0, 0, 0);
            }
            
            DecreaseButton?.Arrange(decreaseButtonRect);
            IncreaseButton?.Arrange(increaseButtonRect);
        }

        return finalSize;
    }

}
