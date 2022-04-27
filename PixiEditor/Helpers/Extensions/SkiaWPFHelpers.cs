using SkiaSharp;
using System.Windows.Media;

namespace PixiEditor.Helpers.Extensions
{
    public static class SkiaWPFHelpers
    {
        public static SKColor ToSKColor(this Color color) => new(color.R, color.G, color.B);

        public static Color ToColor(this SKColor color) => Color.FromRgb(color.Red, color.Green, color.Blue);
    }
}
