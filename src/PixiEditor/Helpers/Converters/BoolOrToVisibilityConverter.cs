using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PixiEditor.Helpers.Converters;
// TODO: seems like this converter is doing the same as the avalonia built in {x:Static BoolConverters.Or}
internal class BoolOrToVisibilityConverter : SingleInstanceMultiValueConverter<BoolOrToVisibilityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value;
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        bool boolean = values.Aggregate(false, (acc, cur) => acc |= (cur as bool?) ?? false);
        return boolean;
    }
}
