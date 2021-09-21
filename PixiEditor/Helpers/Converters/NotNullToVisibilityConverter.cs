using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters
{
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NotNullToVisibilityConverter
        : MarkupConverter
    {
        public bool Inverted { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = value is not null;

            if (Inverted)
            {
                isNull = !isNull;
            }

            return isNull ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
