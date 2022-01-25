using System;
using System.Globalization;
using System.Windows;

namespace PixiEditor.Helpers.Converters
{
    public class EqualityBoolToVisibilityConverter :
        SingleInstanceConverter<EqualityBoolToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            return value.Equals(parameter) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
