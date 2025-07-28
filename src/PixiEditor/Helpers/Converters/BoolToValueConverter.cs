using System.Globalization;
using PixiEditor.UI.Common.Converters;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Helpers.Converters;

internal class BoolToValueConverter : MarkupConverter
{
    public object FalseValue { get; set; }
    
    public object TrueValue { get; set; }
    
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool and true)
        {
            return GetValue(TrueValue, targetType);
        }

        return GetValue(FalseValue, targetType);
    }

    private object GetValue(object value, Type targetType)
    {
        if (value is string s && s.StartsWith("localized:"))
        {
            return new LocalizedString(s.Split("localized:")[1]);
        }

        if (value is string enumString && targetType.IsEnum)
        {
            return Enum.Parse(targetType, enumString);
        }

        return value;
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
