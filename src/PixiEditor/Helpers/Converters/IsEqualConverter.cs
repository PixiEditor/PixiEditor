using System.Globalization;

namespace PixiEditor.Helpers.Converters;

internal class IsEqualConverter : SingleInstanceConverter<IsEqualConverter>
{
    public override object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
            return true;

        if (value.GetType().IsAssignableTo(typeof(Enum)) && parameter is string s)
        {
            parameter = Enum.Parse(value.GetType(), s);
        }
        else
        {
            parameter = System.Convert.ChangeType(parameter, value.GetType());
        }

        return value.Equals(parameter);
    }
}
