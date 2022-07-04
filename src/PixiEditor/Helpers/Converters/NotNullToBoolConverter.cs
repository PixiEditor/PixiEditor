using System;
using System.Globalization;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class NotNullToBoolConverter
        : SingleInstanceConverter<NotNullToBoolConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = value is not null;

            return parameter is null ? result : !result;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}