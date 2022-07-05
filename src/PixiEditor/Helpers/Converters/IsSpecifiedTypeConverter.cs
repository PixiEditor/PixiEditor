using System.Globalization;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters;

[ValueConversion(typeof(object), typeof(bool))]
internal class IsSpecifiedTypeConverter : MarkupConverter
{
    public Type SpecifiedType { get; set; }

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null && value.GetType() == SpecifiedType;
    }
}
