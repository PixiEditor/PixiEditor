using System.Globalization;

namespace PixiEditor.AvaloniaUI.Helpers.Converters;

// TODO: check if this converter can be replaced with StringConverters.IsNullOrEmpty, StringConverters.IsNotNullOrEmpty, ObjectConverters.IsNull, or ObjectConverters.IsNotNull
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
