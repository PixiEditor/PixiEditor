using PixiEditor.Models.DataHolders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace PixiEditor.Helpers.Converters
{
    public class ColorToNotifyableColorConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is NotifyableColor color)
            {
                return color.Color;
            }
            return null;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is Color color)
            {
                return new NotifyableColor(color);
            }
            return null;
        }
    }
}
