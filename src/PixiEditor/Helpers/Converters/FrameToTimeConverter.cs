using System.Globalization;

namespace PixiEditor.Helpers.Converters;

internal class FrameToTimeConverter : SingleInstanceMultiValueConverter<FrameToTimeConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(new[] { value }, targetType, parameter, culture); 
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if(values.Count < 2) return null;
        
        if (values[0] is int frame && values[1] is int fps)
        {
            return TimeSpan.FromSeconds(frame / (double)fps).ToString("mm\\:ss\\.ff");
        }
        
        return null;
    }
}
