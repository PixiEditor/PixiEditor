using System.Globalization;

namespace PixiEditor.Helpers.Converters;

internal class IntPercentConverter : SingleInstanceConverter<IntPercentConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return (int)Math.Round(doubleValue * 100d);
        }
        
        if (value is float floatValue)
        {
            return (int)Math.Round(floatValue * 100f);
        }
        
        if (value is int intValue)
        {
            return intValue * 100;
        }

        return 0;
    }
    
    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return doubleValue / 100d;
        }
        
        if (value is float floatValue)
        {
            return floatValue / 100f;
        }
        
        if (value is int intValue)
        {
            return intValue / 100f;
        }

        return 0;
    }
}
