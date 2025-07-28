using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace PixiEditor.Views.Input;

internal class SwitchItem : AvaloniaObject
{
    public string Content { get; set; } = "";

    public static readonly StyledProperty<IBrush?> BackgroundProperty = AvaloniaProperty.Register<SwitchItem, IBrush?>(
        "Background");

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }
    public object Value { get; set; }

    public BitmapInterpolationMode ScalingMode { get; set; } = BitmapInterpolationMode.HighQuality;

    public SwitchItem(IBrush? background, object value, string content, BitmapInterpolationMode scalingMode = BitmapInterpolationMode.HighQuality)
    {
        Background = background;
        Value = value;
        ScalingMode = scalingMode;
        Content = content;
    }
    public SwitchItem()
    {
    }
}
