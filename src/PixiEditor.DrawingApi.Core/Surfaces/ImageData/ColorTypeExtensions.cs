using System;

namespace PixiEditor.DrawingApi.Core.Surfaces.ImageData;

public static class ColorTypeExtensions
{
    public static int GetBytesPerPixel(this ColorType colorType)
        {
          switch (colorType)
          {
            case ColorType.Unknown:
              return 0;
            case ColorType.Alpha8:
              return 1;
            case ColorType.Rgb565:
              return 2;
            case ColorType.Argb4444:
              return 2;
            case ColorType.Rgba8888:
              return 4;
            case ColorType.Rgb888x:
              return 4;
            case ColorType.Bgra8888:
              return 4;
            case ColorType.Rgba1010102:
              return 4;
            case ColorType.Rgb101010x:
              return 4;
            case ColorType.Gray8:
              return 1;
            case ColorType.RgbaF16:
              return 8;
            case ColorType.RgbaF16Clamped:
              return 8;
            case ColorType.RgbaF32:
              return 16;
            case ColorType.Rg88:
              return 2;
            case ColorType.AlphaF16:
              return 2;
            case ColorType.RgF16:
              return 4;
            case ColorType.Alpha16:
              return 2;
            case ColorType.Rg1616:
              return 4;
            case ColorType.Rgba16161616:
              return 8;
            default:
              throw new ArgumentOutOfRangeException(nameof (colorType));
          }
        }
}
