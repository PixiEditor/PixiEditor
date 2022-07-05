using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PixiEditorPrototype.Converters
{
    internal class ScaleToBitmapScalingModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double scale)
                return DependencyProperty.UnsetValue;
            if (scale < 1)
                return BitmapScalingMode.HighQuality;
            return BitmapScalingMode.NearestNeighbor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
