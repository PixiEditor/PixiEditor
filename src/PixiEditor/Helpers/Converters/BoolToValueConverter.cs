using System.Globalization;

namespace PixiEditor.Helpers.Converters;

internal class BoolToValueConverter : MarkupConverter
{
    public object FalseValue { get; set; }
    
    public object TrueValue { get; set; }
    
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolean && boolean)
        {
            return TrueValue;
        }

        return FalseValue;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == FalseValue)
        {
            return false;
        }

        if (value == TrueValue)
        {
            return true;
        }

        if (targetType == typeof(bool?))
        {
            return null;
        }

        throw new ArgumentException("value was neither FalseValue nor TrueValue and targetType was not a nullable bool");
    }
}
