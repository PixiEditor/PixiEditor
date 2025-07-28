using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data;

namespace PixiEditor.Helpers.Converters;

internal class IndentConverter
    : SingleInstanceConverter<IndentConverter>
{
    private const int IndentSize = 20;

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return new GridLength(((GridLength)value).Value + IndentSize);
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}
