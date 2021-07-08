using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace PixiEditor.Helpers.Converters
{
    public class NullToVisibilityConverter : MarkupExtension, IValueConverter
    {
        private static NullToVisibilityConverter converter;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if(converter == null)
            {
                converter = new NullToVisibilityConverter();
            }
            return converter;
        }
    }
}
