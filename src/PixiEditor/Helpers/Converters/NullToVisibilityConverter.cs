using System.Globalization;

namespace PixiEditor.Helpers.Converters;

internal class NullToVisibilityConverter
    : SingleInstanceConverter<NullToVisibilityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is null;
    }
}
