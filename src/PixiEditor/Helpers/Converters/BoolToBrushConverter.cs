using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PixiEditor.Helpers.Converters;
internal class BoolToBrushConverter : IMultiValueConverter
{
    public IBrush FalseBrush { get; set; } = new SolidColorBrush(Brushes.Black.Color);
    public IBrush TrueBrush { get; set; } = new SolidColorBrush(Brushes.White.Color);

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 1)
        {
            if (values[0] is not bool conv)
                return AvaloniaProperty.UnsetValue;
            return conv ? TrueBrush : FalseBrush;
        }
        else if (values.Count == 2)
        {
            if (values[0] is not bool conv || values[1] is not bool conv2)
                return AvaloniaProperty.UnsetValue;
            return (conv || !conv2) ? TrueBrush : FalseBrush;
        }
        return AvaloniaProperty.UnsetValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
