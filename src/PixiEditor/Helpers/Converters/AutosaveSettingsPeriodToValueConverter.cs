using System.Globalization;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Helpers.Converters;

namespace PixiEditor.Helpers.Converters;

internal class AutosaveSettingsPeriodToValueConverter : MultiValueMarkupConverter
{
    public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return (values[0], values[1]) switch
        {
            (false, _) => new LocalizedString("DISABLED"),
            (true, double d) => new LocalizedString(d.ToString(CultureInfo.InvariantCulture)),
            _ => throw new ArgumentException($"{values[0]} {values[1]} are invalid")
        };
    }

    public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return value switch
        {
            LocalizedString { Key: "DISABLED" } => [false, PreferencesConstants.AutosavePeriodDefault],
            LocalizedString s when double.TryParse(s.Key, out double period) => [true, period],
            _ => throw new ArgumentException($"{value} has invalid type")
        };
    }
}
