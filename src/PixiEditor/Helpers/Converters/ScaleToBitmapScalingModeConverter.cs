using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace PixiEditor.Helpers.Converters;

internal class ScaleToBitmapScalingModeConverter : SingleInstanceConverter<ScaleToBitmapScalingModeConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double scale)
            return DependencyProperty.UnsetValue;
        if (scale < 1)
            return BitmapScalingMode.HighQuality;
        return BitmapScalingMode.NearestNeighbor;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
