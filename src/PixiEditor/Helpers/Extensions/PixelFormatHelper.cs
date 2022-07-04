using SkiaSharp;
using System;
using System.Windows.Media;

namespace PixiEditor.Helpers.Extensions;

public static class PixelFormatHelper
{
    public static SKColorType ToSkia(this PixelFormat format, out SKAlphaType alphaType)
    {
        if (TryToSkia(format, out SKColorType color, out alphaType))
        {
            return color;
        }
        else
        {
            throw new NotImplementedException($"Skia does not support the '{format}' format");
        }
    }

    public static bool TryToSkia(this PixelFormat format, out SKColorType colorType, out SKAlphaType alphaType)
    {
        if (format == PixelFormats.Rgba64)
        {
            alphaType = SKAlphaType.Unpremul;
            colorType = SKColorType.Rgba16161616;
            return true;
        }
        else if (format == PixelFormats.Bgra32)
        {
            alphaType = SKAlphaType.Unpremul;
            colorType = SKColorType.Bgra8888;
            return true;
        }
        else if (format == PixelFormats.Default)
        {
            alphaType = SKAlphaType.Unpremul;
            colorType = SKColorType.RgbaF16;
            return true;
        }
        else if (format == PixelFormats.Gray8)
        {
            alphaType = SKAlphaType.Opaque;
            colorType = SKColorType.Gray8;
            return true;
        }
        else if (format == PixelFormats.Pbgra32)
        {
            alphaType = SKAlphaType.Premul;
            colorType = SKColorType.Bgra8888;
            return true;
        }
        else if (format == PixelFormats.Bgr101010 || format == PixelFormats.Bgr24 || format == PixelFormats.Bgr32 || format == PixelFormats.Bgr555 ||
                 format == PixelFormats.Bgr565 || format == PixelFormats.BlackWhite || format == PixelFormats.Cmyk32 || format == PixelFormats.Gray16 ||
                 format == PixelFormats.Gray2 || format == PixelFormats.Gray32Float || format == PixelFormats.Gray4 || format == PixelFormats.Indexed1 ||
                 format == PixelFormats.Indexed2 || format == PixelFormats.Indexed4 || format == PixelFormats.Indexed8 || format == PixelFormats.Prgba128Float ||
                 format == PixelFormats.Prgba64 || format == PixelFormats.Rgb128Float || format == PixelFormats.Rgb24 || format == PixelFormats.Rgb48 ||
                 format == PixelFormats.Rgba128Float)
        {
            alphaType = SKAlphaType.Unknown;
            colorType = SKColorType.Unknown;
            return false;
        }

        throw new NotImplementedException($"'{format}' has not been implemented by {nameof(PixelFormatHelper)}.{nameof(TryToSkia)}()");
    }

    public static bool IsSkiaSupported(this PixelFormat format)
    {
        return TryToSkia(format, out _, out _);
    }
}