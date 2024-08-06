using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.Numerics;
using HashCode = System.HashCode;

namespace PixiEditor.DrawingApi.Core.Surfaces.ImageData;

public struct ImageInfo : System.IEquatable<ImageInfo>
  {
    /// <summary>An empty <see cref="ImageInfo" />.</summary>
    public static readonly ImageInfo Empty;
    
    /// <summary>The current 32-bit color for the current platform.</summary>
    /// <remarks>On Windows, it is typically <see cref="ColorType.Bgra8888" />, and on Unix-based systems (macOS, Linux) it is typically <see cref="ColorType.Rgba8888" />.</remarks>
    public static readonly ColorType PlatformColorType = DrawingBackendApi.Current.ColorImplementation.GetPlatformColorType();
    
    /// <summary>The number of bits to shift left for the alpha color component.</summary>
    public static readonly int PlatformColorAlphaShift;
    
    /// <summary>The number of bits to shift left for the red color component.</summary>
    public static readonly int PlatformColorRedShift;
    
    /// <summary>The number of bits to shift left for the green color component.</summary>
    public static readonly int PlatformColorGreenShift;
    
    /// <summary>The number of bits to shift left for the blue color component.</summary>
    public static readonly int PlatformColorBlueShift;

    static unsafe ImageInfo()
    {
      DrawingBackendApi.Current.ImageImplementation.GetColorShifts(ref PlatformColorAlphaShift, ref PlatformColorRedShift, ref PlatformColorGreenShift, ref PlatformColorBlueShift);
    }

    /// <summary>Gets or sets the width.</summary>
    /// <value />
    public int Width { get; set; }

    /// <summary>Gets or sets the height.</summary>
    /// <value />
    public int Height { get; set; }

    /// <summary>Gets or sets the color type.</summary>
    /// <value />
    public ColorType ColorType { get; set; }

    /// <summary>Gets the transparency type for the image info.</summary>
    /// <value />
    public AlphaType AlphaType { get; set; }

    /// <summary>Gets or sets the color space.</summary>
    /// <value />
    public ColorSpace? ColorSpace { get; set; }

    public ImageInfo(int width, int height)
    {
      this.Width = width;
      this.Height = height;
      this.ColorType = ImageInfo.PlatformColorType;
      this.AlphaType = AlphaType.Premul;
      this.ColorSpace = null;
    }

    public ImageInfo(int width, int height, ColorType colorType)
    {
      this.Width = width;
      this.Height = height;
      this.ColorType = colorType;
      this.AlphaType = AlphaType.Premul;
      this.ColorSpace = (ColorSpace)null;
    }

    public ImageInfo(int width, int height, ColorType colorType, AlphaType alphaType)
    {
      this.Width = width;
      this.Height = height;
      this.ColorType = colorType;
      this.AlphaType = alphaType;
      this.ColorSpace = (ColorSpace) null;
    }

    public ImageInfo(
      int width,
      int height,
      ColorType colorType,
      AlphaType alphaType,
      ColorSpace colorspace)
    {
      this.Width = width;
      this.Height = height;
      this.ColorType = colorType;
      this.AlphaType = alphaType;
      this.ColorSpace = colorspace;
    }

    /// <summary>Gets the number of bytes used per pixel.</summary>
    /// <value />
    /// <remarks>This is calculated from the <see cref="ImageInfo.ColorType" />. If the color type is <see cref="ColorType.Unknown" />, then the value will be 0.</remarks>
    public readonly int BytesPerPixel => ColorType.GetBytesPerPixel();

    /// <summary>Gets the number of bits used per pixel.</summary>
    /// <value />
    /// <remarks>This is equivalent to multiplying the <see cref="ImageInfo.BytesPerPixel" /> by 8 (the number of bits in a byte).</remarks>
    public readonly int BitsPerPixel => this.BytesPerPixel * 8;

    /// <summary>Gets the total number of bytes needed to store the bitmap data.</summary>
    /// <value />
    /// <remarks>This is calculated as: <see cref="P:SkiaSharp.ImageInfo.Width" /> * <see cref="ImageInfo.Height" /> * <see cref="ImageInfo.BytesPerPixel" />.</remarks>
    public readonly int BytesSize => this.Width * this.Height * this.BytesPerPixel;

    /// <summary>Gets the total number of bytes needed to store the bitmap data as a 64-bit integer.</summary>
    /// <value />
    /// <remarks>This is calculated as: <see cref="P:SkiaSharp.ImageInfo.Width" /> * <see cref="ImageInfo.Height" /> * <see cref="ImageInfo.BytesPerPixel" />.</remarks>
    public readonly long BytesSize64 => (long) this.Width * (long) this.Height * (long) this.BytesPerPixel;

    /// <summary>Gets the number of bytes per row.</summary>
    /// <value />
    /// <remarks>This is calculated as: <see cref="P:SkiaSharp.ImageInfo.Width" /> * <see cref="ImageInfo.BytesPerPixel" />.</remarks>
    public readonly int RowBytes => this.Width * this.BytesPerPixel;

    /// <summary>Gets the number of bytes per row as a 64-bit integer.</summary>
    /// <value />
    /// <remarks>This is calculated as: <see cref="ImageInfo.Width" /> * <see cref="ImageInfo.BytesPerPixel" />.</remarks>
    public readonly long RowBytes64 => (long) this.Width * (long) this.BytesPerPixel;

    /// <summary>Gets a value indicating whether the width or height are less or equal than zero.</summary>
    /// <value />
    public readonly bool IsEmpty => this.Width <= 0 || this.Height <= 0;

    /// <summary>Gets a value indicating whether the configured alpha type is opaque.</summary>
    /// <value />
    public readonly bool IsOpaque => this.AlphaType == AlphaType.Opaque;

    /// <summary>Gets the current size of the image.</summary>
    /// <value />
    public readonly VecI Size => new VecI(this.Width, this.Height);

    /// <summary>Gets a rectangle with the current width and height.</summary>
    /// <value />
    public readonly RectI Rect => RectI.Create(this.Width, this.Height);

    public bool GpuBacked { get; set; } = false;


    public readonly ImageInfo WithSize(VecI size) => this.WithSize(size.X, size.Y);

    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <summary>Creates a new <see cref="ImageInfo" /> with the same properties as this <see cref="ImageInfo" />, but with the specified dimensions.</summary>
    /// <returns>Returns the new <see cref="ImageInfo" />.</returns>
    public readonly ImageInfo WithSize(int width, int height) => this with
    {
      Width = width,
      Height = height
    };

    /// <param name="newColorType">The color type.</param>
    /// <summary>Creates a new <see cref="ImageInfo" /> with the same properties as this <see cref="ImageInfo" />, but with the specified color type.</summary>
    /// <returns>Returns the new <see cref="ImageInfo" />.</returns>
    public readonly ImageInfo WithColorType(ColorType newColorType) => this with
    {
      ColorType = newColorType
    };

    /// <param name="newColorSpace">The color space.</param>
    /// <summary>Creates a new <see cref="ImageInfo" /> with the same properties as this <see cref="ImageInfo" />, but with the specified color space.</summary>
    /// <returns>Returns the new <see cref="ImageInfo" />.</returns>
    public readonly ImageInfo WithColorSpace(ColorSpace newColorSpace) => this with
    {
      ColorSpace = newColorSpace
    };

    /// <param name="newAlphaType">The alpha/transparency type.</param>
    /// <summary>Creates a new <see cref="ImageInfo" /> with the same properties as this <see cref="ImageInfo" />, but with the specified transparency type.</summary>
    /// <returns>Returns the new <see cref="ImageInfo" />.</returns>
    public readonly ImageInfo WithAlphaType(AlphaType newAlphaType) => this with
    {
      AlphaType = newAlphaType
    };
    
    public readonly bool Equals(ImageInfo obj) => this.ColorSpace == obj.ColorSpace && this.Width == obj.Width && this.Height == obj.Height && this.ColorType == obj.ColorType && this.AlphaType == obj.AlphaType;
    
    public override readonly bool Equals(object obj) => obj is ImageInfo ImageInfo && this.Equals(ImageInfo);
    
    public static bool operator ==(ImageInfo left, ImageInfo right) => left.Equals(right);
    
    public static bool operator !=(ImageInfo left, ImageInfo right) => !left.Equals(right);
    
    public override readonly int GetHashCode()
    {
      HashCode hashCode = new HashCode();
      hashCode.Add<ColorSpace>(this.ColorSpace);
      hashCode.Add<int>(this.Width);
      hashCode.Add<int>(this.Height);
      hashCode.Add<ColorType>(this.ColorType);
      hashCode.Add<AlphaType>(this.AlphaType);
      return hashCode.ToHashCode();
    }
  }
