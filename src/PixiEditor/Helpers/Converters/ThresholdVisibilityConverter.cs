using System;
using System.Globalization;
using System.Windows;

namespace PixiEditor.Helpers.Converters
{
    public class ThresholdVisibilityConverter
        : MarkupConverter
    {
        public double Threshold { get; set; } = 100;
        public bool CheckIfLess { get; set; } = false;

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return CheckIfLess
                   ? (double)value < Threshold ? Visibility.Visible : Visibility.Hidden
                   : (double)value >= Threshold ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
