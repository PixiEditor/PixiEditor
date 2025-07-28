using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Media.Imaging;

namespace PixiEditor.Helpers.Converters;

internal class WidthToBitmapScalingModeConverter : SingleInstanceMultiValueConverter<WidthToBitmapScalingModeConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(new [] {value}, targetType, parameter, culture);
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        int? pixelWidth = values[0] as int?;
        double? actualWidth = values[1] as double?;
        if (pixelWidth == null || actualWidth == null)
            return AvaloniaProperty.UnsetValue;
        double zoomLevel = actualWidth.Value / pixelWidth.Value;
        if (zoomLevel < 1)
            return BitmapInterpolationMode.HighQuality;
        return BitmapInterpolationMode.None;
    }
}
