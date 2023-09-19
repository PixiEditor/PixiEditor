using System.Globalization;
using Avalonia;

namespace PixiEditor.AvaloniaUI.Helpers.Converters;
internal class ReciprocalConverter : SingleInstanceConverter<ReciprocalConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double num)
            return AvaloniaProperty.UnsetValue;
        if (parameter is not double mult)
            return 1 / num;
        return mult / num;
    }
}
