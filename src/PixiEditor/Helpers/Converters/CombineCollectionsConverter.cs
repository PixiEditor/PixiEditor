using System.Globalization;

namespace PixiEditor.Helpers.Converters;

internal class CombineCollectionsConverter : SingleInstanceMultiValueConverter<CombineCollectionsConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is IEnumerable<IEnumerable<object>> collections)
        {
            List<object> combined = new List<object>();
            foreach (var collection in collections)
            {
                foreach (var item in collection)
                {
                    if (!combined.Contains(item))
                        combined.Add(item);
                }
            }

            return combined;
        }

        return null;
    }
}
