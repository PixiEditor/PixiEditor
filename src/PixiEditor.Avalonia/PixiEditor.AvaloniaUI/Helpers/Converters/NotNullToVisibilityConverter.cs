using System.Globalization;
using PixiEditor.AvaloniaUI.Helpers.Converters;

namespace PixiEditor.Helpers.Converters;

internal class NotNullToVisibilityConverter
    : MarkupConverter
{
    public bool Inverted { get; set; }

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isNull = IsDefaultValue(value);

        if (Inverted)
        {
            isNull = !isNull;
        }

        return isNull ? false : true;
    }
    
    bool IsDefaultValue(object obj)
    {
        if (obj is null)
        {
            return true;
        }
        
        var type = obj.GetType();

        if (type.IsValueType)
        {
            object defaultValue = Activator.CreateInstance(type);
            return obj.Equals(defaultValue);
        }

        return false;
    }
}
