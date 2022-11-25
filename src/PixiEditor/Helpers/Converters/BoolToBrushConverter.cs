using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PixiEditor.Helpers.Converters;
internal class BoolToBrushConverter : IMultiValueConverter
{
    public Brush FalseBrush { get; set; } = Brushes.Black;
    public Brush TrueBrush { get; set; } = Brushes.White;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 1)
        {
            if (values[0] is not bool conv)
                return DependencyProperty.UnsetValue;
            return conv ? TrueBrush : FalseBrush;
        }
        else if (values.Length == 2)
        {
            if (values[0] is not bool conv || values[1] is not bool conv2)
                return DependencyProperty.UnsetValue;
            return (conv || !conv2) ? TrueBrush : FalseBrush;
        }
        return DependencyProperty.UnsetValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
