using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters
{
    class NotNullToVisibiltyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
