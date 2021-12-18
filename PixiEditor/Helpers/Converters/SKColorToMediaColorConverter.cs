using SkiaSharp;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PixiEditor.Helpers.Converters
{
    class SKColorToMediaColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var skcolor = (SKColor)value;
            return Color.FromArgb(skcolor.Alpha, skcolor.Red, skcolor.Green, skcolor.Blue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (Color)value;
            return new SKColor(color.R, color.G, color.B, color.A);
        }
    }
}
