using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters
{
    public class ZoomToViewportConverter
        : SingleInstanceConverter<ZoomToViewportConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double scale)
            {
                double newSize = Math.Clamp((double)parameter / scale, 1, 9999);
                return new Rect(0, 0, newSize, newSize);
            }

            return Binding.DoNothing;
        }
    }
}
