using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters
{
    class ThresholdVisibilityConverter : IValueConverter
    {
        public double Threshold { get; set; } = 100;
        public bool CheckIfLess { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (CheckIfLess)
                return (double)value < Threshold ? Visibility.Visible : Visibility.Hidden;
            else
                return (double)value >= Threshold ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
