using PixiEditor.Models.DataHolders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace PixiEditor.Helpers.Converters
{
    public class NotifyableColorToHexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is NotifyableColor color)
            {
                return color.Color.ToString();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return new NotifyableColor((Color)ColorConverter.ConvertFromString((string)value));
            }
            catch
            {
                return null;
            }

        }
    }
}
