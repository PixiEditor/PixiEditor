using System.Globalization;
using PixiEditor.Extensions.Helpers;
using PixiEditor.Helpers.Converters;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Helpers;

internal class EnumDescriptionConverter : SingleInstanceConverter<EnumDescriptionConverter>
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
