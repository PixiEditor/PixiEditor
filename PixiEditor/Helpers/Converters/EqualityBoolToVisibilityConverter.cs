using System;
using System.Globalization;
using System.Windows;

namespace PixiEditor.Helpers.Converters
{
    public class EqualityBoolToVisibilityConverter : MarkupConverter
    {
        public bool Invert { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Invert;

            var parameterValue = System.Convert.ChangeType(parameter, value.GetType());
            return value.Equals(parameterValue) != Invert ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
