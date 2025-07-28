using System.Globalization;
using PixiEditor.UI.Common.Converters;

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
