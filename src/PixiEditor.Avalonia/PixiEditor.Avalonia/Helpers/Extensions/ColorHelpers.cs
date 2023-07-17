using System.Windows.Media;
using Avalonia.Media;
using PixiEditor.Extensions.Palettes;
using BackendColor = PixiEditor.DrawingApi.Core.ColorsImpl.Color;

namespace PixiEditor.Helpers.Extensions;

internal static class ColorHelpers
{
    public static BackendColor ToOpaqueColor(this Color color) => new(color.R, color.G, color.B);
    public static BackendColor ToColor(this Color color) => new(color.R, color.G, color.B, color.A);
    public static BackendColor ToColor(this PaletteColor color) => new(color.R, color.G, color.B, 255);

    public static PaletteColor ToPaletteColor(this Color color) => new(color.R, color.G, color.B);
    public static PaletteColor ToPaletteColor(this BackendColor color) => new(color.R, color.G, color.B);

    public static Color ToOpaqueMediaColor(this BackendColor color) => Color.FromRgb(color.R, color.G, color.B);
    public static Color ToColor(this BackendColor color) => Color.FromArgb(color.A, color.R, color.G, color.B);
    public static Color ToMediaColor(this PaletteColor color) => Color.FromRgb(color.R, color.G, color.B);
    
    public static BackendColor BlendColors(BackendColor bottomColor, BackendColor topColor)
    {
        if (topColor.A is < 255 and > 0)
        {
            byte r = (byte)((topColor.R * topColor.A / 255) + (bottomColor.R * bottomColor.A * (255 - topColor.A) / (255 * 255)));
            byte g = (byte)((topColor.G * topColor.A / 255) + (bottomColor.G * bottomColor.A * (255 - topColor.A) / (255 * 255)));
            byte b = (byte)((topColor.B * topColor.A / 255) + (bottomColor.B * bottomColor.A * (255 - topColor.A) / (255 * 255)));
            byte a = (byte)(topColor.A + (bottomColor.A * (255 - topColor.A) / 255));
            return new BackendColor(r, g, b, a);
        }

        return topColor.A == 255 ? topColor : bottomColor;
    }
}
