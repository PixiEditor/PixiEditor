using System.Globalization;

namespace PixiEditor.Helpers.Converters;

internal class SubtractConverter : SingleInstanceConverter<SubtractConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        object parsedValue = value is string stringValue ? double.Parse(stringValue) : value;
        object parsedParameter = parameter is string parameterString ? double.Parse(parameterString) : parameter;
        
        if (parsedValue is not double doubleValue)
        {
            return value;
        }

        if (parsedParameter is not double doubleParameter)
        {
            return value;
        }

        return doubleValue - doubleParameter;
    }
}
