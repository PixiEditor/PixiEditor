using System.Windows.Media;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.Helpers.Extensions;

internal static class PixelFormatHelper
{
    public static ColorType ToColorType(this PixelFormat format, out AlphaType alphaType)
    {
        if (TryConvertToColorType(format, out var color, out alphaType))
        {
            return color;
        }

        throw new NotImplementedException($"Skia does not support the '{format}' format");
    }

    public static bool TryConvertToColorType(this PixelFormat format, out ColorType colorType, out AlphaType alphaType)
    {
        if (format == PixelFormats.Rgba64)
        {
            alphaType = AlphaType.Unpremul;
            colorType = ColorType.Rgba16161616;
            return true;
        }

        if (format == PixelFormats.Bgra32)
        {
            alphaType = AlphaType.Unpremul;
            colorType = ColorType.Bgra8888;
            return true;
        }

        if (format == PixelFormats.Default)
        {
            alphaType = AlphaType.Unpremul;
            colorType = ColorType.RgbaF16;
            return true;
        }

        if (format == PixelFormats.Gray8)
        {
            alphaType = AlphaType.Opaque;
            colorType = ColorType.Gray8;
            return true;
        }

        if (format == PixelFormats.Pbgra32)
        {
            alphaType = AlphaType.Premul;
            colorType = ColorType.Bgra8888;
            return true;
        }

        if (format == PixelFormats.Bgr101010 || format == PixelFormats.Bgr24 || format == PixelFormats.Bgr32 ||
            format == PixelFormats.Bgr555 ||
            format == PixelFormats.Bgr565 || format == PixelFormats.BlackWhite || format == PixelFormats.Cmyk32 ||
            format == PixelFormats.Gray16 ||
            format == PixelFormats.Gray2 || format == PixelFormats.Gray32Float || format == PixelFormats.Gray4 ||
            format == PixelFormats.Indexed1 ||
            format == PixelFormats.Indexed2 || format == PixelFormats.Indexed4 || format == PixelFormats.Indexed8 ||
            format == PixelFormats.Prgba128Float ||
            format == PixelFormats.Prgba64 || format == PixelFormats.Rgb128Float || format == PixelFormats.Rgb24 ||
            format == PixelFormats.Rgb48 ||
            format == PixelFormats.Rgba128Float)
        {
            alphaType = AlphaType.Unknown;
            colorType = ColorType.Unknown;
            return false;
        }

        throw new NotImplementedException(
            $"'{format}' has not been implemented by {nameof(PixelFormatHelper)}.{nameof(TryConvertToColorType)}()");
    }

    public static bool IsSkiaSupported(this PixelFormat format)
    {
        return TryConvertToColorType(format, out _, out _);
    }
}
