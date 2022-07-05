using System.Windows.Media;
using SkiaSharp;

namespace PixiEditor.Helpers.Extensions;

internal static class SkiaWPFHelpers
{
    public static SKColor ToOpaqueSKColor(this Color color) => new(color.R, color.G, color.B);
    public static SKColor ToSKColor(this Color color) => new(color.R, color.G, color.B, color.A);

    public static Color ToOpaqueColor(this SKColor color) => Color.FromRgb(color.Red, color.Green, color.Blue);
    public static Color ToColor(this SKColor color) => Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
}
