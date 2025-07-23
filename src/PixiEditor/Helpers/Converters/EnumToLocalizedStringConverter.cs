using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using PixiEditor.Extensions.Helpers;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Helpers.Converters;

internal class EnumToLocalizedStringConverter : SingleInstanceConverter<EnumToLocalizedStringConverter>
{
    private Dictionary<object, string> enumTranslations = new(
        typeof(EnumToLocalizedStringConverter).Assembly
            .GetCustomAttributes()
            .OfType<ILocalizeEnumInfo>()
            .Select(x => new KeyValuePair<object, string>(x.GetEnumValue(), x.LocalizationKey)));
    
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Enum enumValue)
        {
            return value;
        }

        if (enumTranslations.TryGetValue(enumValue, out var assemblyDefinedKey))
        {
            return assemblyDefinedKey;
        }

        if (EnumHelpers.HasDescription(enumValue))
        {
            return EnumHelpers.GetDescription(enumValue);
        }

        ThrowUntranslatedEnumValue(enumValue);
        return enumValue;
    }

    [Conditional("DEBUG")]
    private static void ThrowUntranslatedEnumValue(object value)
    {
        throw new ArgumentException(
            $"Enum value '{value.GetType()}.{value}' has no value defined. Either add a Description attribute to the enum values or a LocalizeEnum attribute in EnumTranslations.cs for third party enums",
            nameof(value));
    }
}
