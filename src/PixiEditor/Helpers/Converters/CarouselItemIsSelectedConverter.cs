using System.Globalization;

namespace PixiEditor.Helpers.Converters;

internal class CarouselItemIsSelectedConverter : SingleInstanceMultiValueConverter<CarouselItemIsSelectedConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(new[] { value }, targetType, parameter, culture);
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2)
            throw new ArgumentException("Exactly two values must be provided.");

        var item = values[0];
        var selectedItem = values[1];

        return item != null && item.Equals(selectedItem);
    }
}
