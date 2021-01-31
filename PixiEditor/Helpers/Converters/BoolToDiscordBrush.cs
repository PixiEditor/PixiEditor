using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PixiEditor.Helpers.Converters
{
    class BoolToDiscordBrush : IValueConverter
    {
        private static SolidColorBrush IsPlayingBrush = new SolidColorBrush(Color.FromRgb(114, 137, 218));
        private static SolidColorBrush IsntPlayingBrush = new SolidColorBrush(Color.FromRgb(32, 34, 37));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? IsPlayingBrush : IsntPlayingBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}