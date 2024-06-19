using Avalonia;
using Avalonia.Controls;

namespace PixiEditor.AvaloniaUI.Views.Animations;

public class TimelineSlider : Slider
{
    public static readonly StyledProperty<double> ScaleProperty = AvaloniaProperty.Register<TimelineSlider, double>(
        nameof(Scale), 100);

    public double Scale
    {
        get => GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
    
    protected override Type StyleKeyOverride => typeof(TimelineSlider);
}
