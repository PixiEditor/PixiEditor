using System.Globalization;
using Avalonia.Media.Imaging;
using PixiEditor.UI.Common.Converters;

namespace PixiEditor.Helpers.Converters;

internal class BoolToBitmapValueConverter : MarkupConverter
{
    public Bitmap FalseValue { get; set; }
    
    public Bitmap TrueValue { get; set; }
    
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool and true)
        {
            return GetValue(TrueValue);
        }

        return GetValue(FalseValue);
    }

    private Bitmap? GetValue(object value)
    {
        if (value is string path)
        {
            return ImagePathToBitmapConverter.TryLoadBitmapFromRelativePath(path);
        }

        return value as Bitmap;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == FalseValue)
        {
            return false;
        }

        if (value == TrueValue)
        {
            return true;
        }

        if (targetType == typeof(bool?))
        {
            return null;
        }

        throw new ArgumentException("value was neither FalseValue nor TrueValue and targetType was not a nullable bool");
    }
}
