using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters;

[ValueConversion(typeof(object), typeof(Visibility))]
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

        return isNull ? Visibility.Collapsed : Visibility.Visible;
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
