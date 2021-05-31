using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters
{
    public class AlternativeTextConverter : IValueConverter
    {
        public bool OnlyEmptyString { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (OnlyEmptyString && string.IsNullOrEmpty((string)value))
            {
                return parameter;
            }
            else if (string.IsNullOrWhiteSpace((string)value))
            {
                return parameter;
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
