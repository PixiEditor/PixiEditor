using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters
{
    public class IntToViewportRectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string and "vertical")
            {
                return new Rect(0, 0, 1d / (int)value, 1d);
            }

            return new Rect(0, 0, 1d, 1d / (int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}