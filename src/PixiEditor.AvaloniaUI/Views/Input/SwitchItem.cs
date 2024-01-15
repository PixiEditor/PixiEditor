using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace PixiEditor.AvaloniaUI.Views.Input;

internal class SwitchItem
{
    public string Content { get; set; } = "";
    public IBrush? Background { get; set; }
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
