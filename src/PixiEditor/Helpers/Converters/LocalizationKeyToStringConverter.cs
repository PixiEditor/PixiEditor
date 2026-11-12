using System.Globalization;
using System.Text;
using Avalonia.Input;
using PixiEditor.OperatingSystem;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Helpers.Converters;

internal class LocalizationKeyToStringConverter
    : SingleInstanceConverter<LocalizationKeyToStringConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            string val => new LocalizedString(val).Value,
            _ => value
        };
    }
}
