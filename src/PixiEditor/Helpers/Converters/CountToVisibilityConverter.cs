using System.Globalization;

namespace PixiEditor.Helpers.Converters;

// TODO: the same should be doable by binding to the int directly and using a bang, e.g. {Binding !#someElem.Count}
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
