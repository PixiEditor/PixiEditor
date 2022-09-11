using System.Windows.Media;
using BackendColor = PixiEditor.DrawingApi.Core.ColorsImpl.Color;

namespace PixiEditor.Helpers.Extensions;

internal static class ColorHelpers
{
    public static BackendColor ToOpaqueColor(this Color color) => new(color.R, color.G, color.B);
    public static BackendColor ToColor(this Color color) => new(color.R, color.G, color.B, color.A);

    public static Color ToOpaqueMediaColor(this BackendColor color) => Color.FromRgb(color.R, color.G, color.B);
    public static Color ToColor(this BackendColor color) => Color.FromArgb(color.A, color.R, color.G, color.B);
}
