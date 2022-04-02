using System;
using System.Globalization;
using System.Windows.Data;

namespace PixiEditor.Zoombox
{
    internal class BoolToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value is not bool converted)
                return 1;
            return converted ? -1 : 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
