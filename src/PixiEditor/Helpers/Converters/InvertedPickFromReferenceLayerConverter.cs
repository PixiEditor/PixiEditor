using System.Globalization;
using PixiEditor.Models.Handlers.Tools;

namespace PixiEditor.Helpers.Converters;

internal class InvertedPickFromReferenceLayerConverter : SingleInstanceConverter<InvertedPickFromReferenceLayerConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IColorPickerHandler handler)
        {
            return !handler.PickFromReferenceLayer;
        }

        return false;
    }
}
