using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters
{
    [ValueConversion(typeof(string), typeof(int))]
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

            return int.Parse(match.Groups[0].ValueSpan);
        }
    }
}
