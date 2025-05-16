using System.Globalization;
using PixiEditor.Extensions.Helpers;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Helpers.Converters;

internal class EnumToLocalizedStringConverter : SingleInstanceConverter<EnumToLocalizedStringConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue)
        {
            if (EnumHelpers.HasDescription(enumValue))
            {
                return EnumHelpers.GetDescription(enumValue);
            }

            return ToLocalizedStringFormat(enumValue);
        }

        return value;
    }

    private string ToLocalizedStringFormat(Enum enumValue)
    {
        // VALUE_ENUMTYPE
        // for example BlendMode.Normal becomes NORMAL_BLEND_MODE

        string enumType = enumValue.GetType().Name;

        string value = enumValue.ToString();

        return $"{value.ToSnakeCase()}_{enumType.ToSnakeCase()}".ToUpper();
    }
}
