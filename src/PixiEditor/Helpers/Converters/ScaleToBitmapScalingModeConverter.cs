using System.Globalization;
using Avalonia.Media.Imaging;

namespace PixiEditor.Helpers.Converters;

internal class ScaleToBitmapScalingModeConverter : SingleInstanceConverter<ScaleToBitmapScalingModeConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double scale)
            return BitmapInterpolationMode.None;
        return Calculate(scale);
    }

    public static BitmapInterpolationMode Calculate(double scale)
    {
        if (scale < 1)
            return BitmapInterpolationMode.HighQuality;
        return BitmapInterpolationMode.None;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
