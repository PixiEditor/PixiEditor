using System.Globalization;
using ColorPicker.Models;

namespace PixiEditor.Helpers.Converters;

internal class IntToPickerTypeConverter
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
