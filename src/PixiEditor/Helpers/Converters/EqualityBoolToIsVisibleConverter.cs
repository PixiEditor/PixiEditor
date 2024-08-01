using System.Globalization;
using PixiEditor.UI.Common.Converters;

namespace PixiEditor.Helpers.Converters;

/// TODO: I refactored this logic in <see cref="IsEqualConverter"/>, all usages of this converter should be replaced with IsEqualConverter
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
