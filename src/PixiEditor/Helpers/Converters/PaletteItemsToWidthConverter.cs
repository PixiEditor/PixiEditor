using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Helpers.Converters
{
    public class PaletteItemsToWidthConverter : SingleInstanceConverter<PaletteItemsToWidthConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is IList<SKColor> colors && colors.Count == 0)
            {
                return 0;
            }

            return 120;
        }
    }
}
