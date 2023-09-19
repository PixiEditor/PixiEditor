using System.Globalization;

namespace PixiEditor.AvaloniaUI.Helpers.Converters;

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
