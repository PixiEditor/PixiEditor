using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace PixiEditor.Helpers.Converters
{
    public class BoolToIntConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString() == "0";
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
                if (boolean == false)
                    return 0;
            return 1;
        }
    }
}