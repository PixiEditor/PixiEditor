using System;
using System.Globalization;

namespace PixiEditor.Helpers.Converters
{
    public class DoubleToIntConverter :
        SingleInstanceConverter<DoubleToIntConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double or float)
            {
                double val = (double)value;
                return (int)val;
            }

            return value;
        }
    }
}
