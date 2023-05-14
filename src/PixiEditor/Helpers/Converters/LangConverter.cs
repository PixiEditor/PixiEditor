using System.Globalization;
using PixiEditor.Models.Localization;

namespace PixiEditor.Helpers.Converters;

internal class LangConverter : SingleInstanceConverter<LangConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string key)
        {
            return new LocalizedString(key);
        }

        return value;
    }
}
