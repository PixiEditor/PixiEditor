using System;
using System.Globalization;
using System.Windows;

namespace PixiEditor.Helpers.Converters
{
    public class NullToVisibilityConverter
        : SingleInstanceConverter<NullToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is null ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
