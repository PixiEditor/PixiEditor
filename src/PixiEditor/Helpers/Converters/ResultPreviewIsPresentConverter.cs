using System.Globalization;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Helpers.Converters;

internal class ResultPreviewIsPresentConverter : SingleInstanceMultiValueConverter<ResultPreviewIsPresentConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TexturePreview preview)
        {
            return preview.Preview != null;
        }

        return false;
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is TexturePreview preview)
        {
            return preview.Preview != null;
        }

        return false;
    }
}
