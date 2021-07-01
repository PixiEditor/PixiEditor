using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace PixiEditor.Helpers.Converters
{
    public class FormattedColorConverter : MarkupExtension, IMultiValueConverter
    {
        private static FormattedColorConverter converter;
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values != null && values.Length > 1 && values[0] is Color color && values[1] is string format)
            {
                switch (format.ToLower())
                {
                    case "hex":
                        return color.ToString();
                    case "rgba":
                        return $"({color.R}, {color.G}, {color.B}, {color.A})";
                    default:
                        break;
                }
            }

            return "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if(converter == null)
            {
                converter = new FormattedColorConverter();
            }
            return converter;
        }
    }
}
