using Avalonia.Platform;
using Drawie.Backend.Core.Surfaces.ImageData;

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
    
    public static ColorType ToColorType(this SKColorType colorType)
    { 
        return (ColorType)colorType;
    }
    
    public static AlphaType ToAlphaType(this SKAlphaType alphaType)
    {
        return (AlphaType)alphaType;
    }

    public static bool TryConvertToColorType(this PixelFormat format, out ColorType colorType, out AlphaType alphaType)
    {
        if (format == PixelFormats.Rgba64)
        {
            alphaType = AlphaType.Premul;
            colorType = ColorType.Rgba16161616;
            return true;
        }

        if (format == PixelFormats.Bgra8888)
        {
            alphaType = AlphaType.Premul;
            colorType = ColorType.Bgra8888;
            return true;
        }

        if (format == PixelFormats.Rgba8888)
        {
            alphaType = AlphaType.Premul;
            colorType = ColorType.Rgba8888;
            return true;
        }

        /*if (format == PixelFormats.Bgra32)
        {
            alphaType = AlphaType.Unpremul;
            colorType = ColorType.Bgra8888;
            return true;
        }*/

        /*if (format == PixelFormats.Default)
        {
            alphaType = AlphaType.Unpremul;
            colorType = ColorType.RgbaF16;
            return true;
        }*/

        if (format == PixelFormats.Gray8)
        {
            alphaType = AlphaType.Opaque;
            colorType = ColorType.Gray8;
            return true;
        }

        /*if (format == PixelFormats.Pbgra32)
        {
            alphaType = AlphaType.Premul;
            colorType = ColorType.Bgra8888;
            return true;
        }*/

        if (format == PixelFormats.Bgr24 || format == PixelFormats.BlackWhite ||
            format == PixelFormats.Gray16 ||
            format == PixelFormats.Gray2 || format == PixelFormats.Gray32Float || format == PixelFormats.Gray4
            || format == PixelFormats.Rgb24)
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
