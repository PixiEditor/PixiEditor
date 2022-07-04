using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters;

public class IndentConverter
    : SingleInstanceConverter<IndentConverter>
{
    private const int IndentSize = 20;

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return new GridLength(((GridLength)value).Value + IndentSize);
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}