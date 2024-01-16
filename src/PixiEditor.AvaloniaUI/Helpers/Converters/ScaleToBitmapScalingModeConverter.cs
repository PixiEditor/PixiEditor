using System.Globalization;
using Avalonia.Media.Imaging;
using PixiEditor.UI.Common.Converters;

namespace PixiEditor.AvaloniaUI.Helpers.Converters;

internal class ScaleToBitmapScalingModeConverter : SingleInstanceConverter<ScaleToBitmapScalingModeConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double scale)
            return BitmapInterpolationMode.None;
        if (scale < 1)
            return BitmapInterpolationMode.HighQuality;
        return BitmapInterpolationMode.None;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
