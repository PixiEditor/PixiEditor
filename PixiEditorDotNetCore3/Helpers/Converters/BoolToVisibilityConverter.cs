using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value.ToString() == "Hidden" || value.ToString() == "Collapsed")
            {
                return false;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool)
            {
                if((bool)value == false)
                {
                    return "Hidden";
                }
            }
            return "Visible";
        }
    }
}
