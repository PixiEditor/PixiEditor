using System.Globalization;

namespace PixiEditor.AvaloniaUI.Helpers.Converters;

internal class CountToVisibilityConverter : SingleInstanceConverter<CountToVisibilityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intVal)
        {
            return intVal == 0;
        }

        return true;
    }
}
