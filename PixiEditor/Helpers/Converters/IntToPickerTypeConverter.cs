using ColorPicker.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters
{
    public class IntToPickerTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (PickerType)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value;
        }
    }
}
