using ColorPicker.Models;
using System;
using System.Globalization;

namespace PixiEditor.Helpers.Converters;

public class IntToPickerTypeConverter
    : SingleInstanceConverter<IntToPickerTypeConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (PickerType)value;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (int)value;
    }
}