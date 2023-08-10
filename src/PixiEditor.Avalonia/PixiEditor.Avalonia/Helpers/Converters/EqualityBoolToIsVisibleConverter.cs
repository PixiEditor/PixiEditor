using System.Globalization;
using System.Windows;

namespace PixiEditor.Helpers.Converters;

internal class EqualityBoolToIsVisibleConverter : MarkupConverter
{
    public bool Invert { get; set; }

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return Invert;

        if (value.GetType().IsAssignableTo(typeof(Enum)) && parameter is string s)
        {
            parameter = Enum.Parse(value.GetType(), s);
        }
        else
        {
            parameter = System.Convert.ChangeType(parameter, value.GetType());
        }

        return value.Equals(parameter) != Invert ? true : false;
    }
}
