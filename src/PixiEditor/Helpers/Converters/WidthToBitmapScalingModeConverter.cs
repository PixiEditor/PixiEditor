using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace PixiEditor.Helpers.Converters
{
    internal class WidthToBitmapScalingModeConverter : SingleInstanceMultiValueConverter<WidthToBitmapScalingModeConverter>
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int? pixelWidth = values[0] as int?;
            double? actualWidth = values[1] as double?;
            if (pixelWidth == null || actualWidth == null)
                return DependencyProperty.UnsetValue;
            double zoomLevel = actualWidth.Value / pixelWidth.Value;
            if (zoomLevel < 1)
                return BitmapScalingMode.HighQuality;
            return BitmapScalingMode.NearestNeighbor;
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
