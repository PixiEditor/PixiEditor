using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters
{
    public class AlternativeFontStyleConverter : IValueConverter
    {
        public bool OnlyEmptyString { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (OnlyEmptyString && string.IsNullOrEmpty((string)value))
            {
                return FontStyles.Italic;
            }
            else if (string.IsNullOrWhiteSpace((string)value))
            {
                return FontStyles.Italic;
            }
            else
            {
                return FontStyles.Normal;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
