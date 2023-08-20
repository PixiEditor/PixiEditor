using System.Globalization;
using System.Windows;
using PixiEditor.AvaloniaUI.Helpers.Converters;

namespace PixiEditor.Helpers.Converters;

internal class ThresholdVisibilityConverter
    : MarkupConverter
{
    public double Threshold { get; set; } = 100;
    public bool CheckIfLess { get; set; } = false;

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return CheckIfLess
            ? (double)value < Threshold
            : (double)value >= Threshold;
    }
}
