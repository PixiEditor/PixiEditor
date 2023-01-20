using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Helpers.Converters;
internal class EmptyStringFillerConverter : MarkupConverter
{
    public string NullText { get; set; } = "[null]";

    public string EmptyText { get; set; } = "[empty]";

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            string s => s.Length switch
            {
                0 => EmptyText,
                _ => s
            },
            _ => NullText
        };
    }
}
