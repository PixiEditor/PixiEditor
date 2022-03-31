using System;
using System.Globalization;
using System.Windows.Media;
using SkiaSharp;

namespace PixiEditor.Helpers.Converters
{
    public class ReplaceColorsPackConverter : SingleInstanceMultiValueConverter<ReplaceColorsPackConverter>
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            SKColor first = (SKColor)values[0];
            Color rawSecond = (Color)values[1];

            SKColor second = new SKColor(rawSecond.R, rawSecond.G, rawSecond.B, rawSecond.A);

            return (first, second);
        }
    }
}
