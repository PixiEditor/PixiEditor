using System.Globalization;
using PixiEditor.Extensions.Helpers;

namespace PixiEditor.Helpers.Converters;

internal class EnumToLocalizedStringConverter : SingleInstanceConverter<EnumToLocalizedStringConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue)
        {
            return EnumHelpers.GetDescription(enumValue);
        }

        return value;
    }
}
