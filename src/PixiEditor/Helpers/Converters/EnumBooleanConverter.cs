using Avalonia;

namespace PixiEditor.Helpers.Converters;

internal class EnumBooleanConverter : SingleInstanceConverter<EnumBooleanConverter>
{
    #region IValueConverter Members
    public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        string parameterString = parameter as string;
        if (parameterString == null)
            return AvaloniaProperty.UnsetValue;

        if (Enum.IsDefined(value.GetType(), value) == false)
            return AvaloniaProperty.UnsetValue;

        object parameterValue = Enum.Parse(value.GetType(), parameterString);

        return parameterValue.Equals(value);
    }

    public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        string parameterString = parameter as string;
        if (parameterString == null)
            return AvaloniaProperty.UnsetValue;

        if ((bool)value)
        {
            return Enum.Parse(targetType, parameterString);
        }

        return AvaloniaProperty.UnsetValue;
    }
    #endregion
}
