using System.Windows.Data;

namespace PixiEditor.Helpers.Converters;

[ValueConversion(typeof(bool), typeof(bool))]
internal class InverseBooleanConverter
    : SingleInstanceConverter<InverseBooleanConverter>
{
    public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return targetType != typeof(bool)
            ? throw new InvalidOperationException("The target must be a boolean")
            : !(bool)value;
    }
}
