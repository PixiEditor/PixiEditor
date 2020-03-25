using System;
using System.Globalization;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value.ToString() == "Transparent")
            {
                return false;
            }
            return true;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool)
            {
                if((bool)value == false)
                {
                    return "Transparent";
                }
            }
            return "#638DCA";
        }
    }
}
