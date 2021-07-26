using System;
using System.Globalization;
using System.Linq;
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
            return string.Format("{0} {1}", value, "px");
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value as string))
            {
                return null;
            }

            string slicedString = value.ToString().Split(' ').First();
            slicedString = Regex.Replace(slicedString, "\\p{L}", string.Empty);
            if (slicedString == string.Empty)
            {
                return null;
            }

            return int.Parse(slicedString);
        }
    }
}