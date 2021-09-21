using System;
using System.Globalization;

namespace PixiEditor.Helpers.Converters
{
    public class BoolToBrushConverter
        : SingleInstanceConverter<BoolToBrushConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BrushTuple tuple = (BrushTuple)parameter;
            return (bool)value ? tuple.FirstBrush : tuple.SecondBrush;
        }
    }
}