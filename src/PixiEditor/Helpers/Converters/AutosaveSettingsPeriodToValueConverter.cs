using System.Globalization;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Helpers.Converters;

namespace PixiEditor.Helpers.Converters;

internal class AutosaveSettingsPeriodToValueConverter : SingleInstanceConverter<AutosaveSettingsPeriodToValueConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            -1.0 => new LocalizedString("DISABLED"),
            double d => new LocalizedString(d.ToString(CultureInfo.InvariantCulture)),
            _ => throw new ArgumentException($"{value} has invalid type")
        };
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            LocalizedString { Key: "DISABLED" } => -1,
            LocalizedString s when double.TryParse(s.Key, out double period) => period,
            _ => throw new ArgumentException($"{value} has invalid type")
        };
    }
}
