using System;
using System.Globalization;
using System.Windows.Media;

namespace PixiEditor.Helpers.Converters
{
    internal class ZoomLevelToBitmapScalingModeConverter : SingleInstanceConverter<ZoomLevelToBitmapScalingModeConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double zoomLevel = (double)value;
            if (zoomLevel < 1)
                return BitmapScalingMode.HighQuality;
            return BitmapScalingMode.NearestNeighbor;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
