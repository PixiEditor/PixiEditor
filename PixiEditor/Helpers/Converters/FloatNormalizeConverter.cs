using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters
{
    public class FloatNormalizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => 
            (float)value * 100;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => 
            (float) value / 100;
    }
}
