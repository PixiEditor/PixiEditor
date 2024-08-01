using System.Globalization;
using PixiEditor.UI.Common.Converters;

namespace PixiEditor.Helpers.Converters;

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

        return !isNull;
    }
    
    bool IsDefaultValue(object obj)
    {
        if (obj is null)
        {
            return true;
        }
        
        var type = obj.GetType();

        //TODO: Try to find what breaks without below, because in my opinion this
        // is not a correct thing to do with NotNull converter, lots with false positives, like 0 as a int
        /*if (type.IsValueType)
        {
            object defaultValue = Activator.CreateInstance(type);
            return obj.Equals(defaultValue);
        }*/

        return false;
    }
}
