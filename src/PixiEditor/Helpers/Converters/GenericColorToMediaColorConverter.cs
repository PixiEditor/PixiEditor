using System.Globalization;
using Avalonia.Media;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Extensions.CommonApi.Palettes;
using BackendColor = Drawie.Backend.Core.ColorsImpl.Color;

namespace PixiEditor.Helpers.Converters;

internal class GenericColorToMediaColorConverter : SingleInstanceConverter<GenericColorToMediaColorConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Color color = default;
        if (value is BackendColor backendColor)
        {
            color = backendColor.ToColor();
        }
        else if (value is PaletteColor paletteColor)
        {
            color = paletteColor.ToMediaColor();
        }

        if (targetType == typeof(Brush))
        {
            return new SolidColorBrush(color);
        }

        return color;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var color = (Color)value;
        return new BackendColor(color.R, color.G, color.B, color.A);
    }
}
