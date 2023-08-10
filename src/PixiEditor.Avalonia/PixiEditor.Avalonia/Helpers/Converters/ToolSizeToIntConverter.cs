using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PixiEditor.Helpers.Converters;

internal class ToolSizeToIntConverter
    : SingleInstanceConverter<ToolSizeToIntConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value.ToString();
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string s)
        {
            return null;
        }

        Match match = Regex.Match(s, @"\d+");

        if (!match.Success)
        {
            return null;
        }

        if (int.TryParse(match.Groups[0].ValueSpan.ToString().Normalize(NormalizationForm.FormKC), out int result))
        {
            return result;
        }

        return null;
    }
}
