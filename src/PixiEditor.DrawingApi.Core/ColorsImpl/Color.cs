using System;
using System.Globalization;

namespace PixiEditor.DrawingApi.Core.ColorsImpl
{
   /// <summary>32-bit ARGB unpremultiplied color value.</summary>
  /// <remarks>The color components are always in a known order.</remarks>
  public readonly struct Color : IEquatable<Color>
  {
    /// <summary>Gets an "empty" color, with zero for all the components.</summary>
    public static readonly Color Empty = default;
    private readonly uint _colorValue;

    public Color(uint value) => this._colorValue = value;
    public Color(byte red, byte green, byte blue, byte alpha) => this._colorValue = (uint) ((int) alpha << 24 | (int) red << 16 | (int) green << 8) | (uint) blue;

    public Color(byte red, byte green, byte blue) => this._colorValue = (uint) (-16777216 | (int) red << 16 | (int) green << 8) | (uint) blue;

    /// <param name="red">The new red component.</param>
    /// <summary>Returns a new color based on this current instance, but with the new red channel value.</summary>
    public Color WithRed(byte red) => new Color(red, this.G, this.B, this.A);

    /// <param name="green">The new green component.</param>
    /// <summary>Returns a new color based on this current instance, but with the new green channel value.</summary>
    public Color WithGreen(byte green) => new Color(this.R, green, this.B, this.A);

    /// <param name="blue">The new blue component.</param>
    /// <summary>Returns a new color based on this current instance, but with the new blue channel value.</summary>
    public Color WithBlue(byte blue) => new Color(this.R, this.G, blue, this.A);

    /// <param name="alpha">The new alpha component.</param>
    /// <summary>Returns a new color based on this current instance, but with the new alpha channel value.</summary>
    public Color WithAlpha(byte alpha) => new Color(this.R, this.G, this.B, alpha);

    /// <summary>Gets the alpha component of the color.</summary>
    /// <value />
    public byte A => (byte)(this._colorValue >> 24 & (uint) byte.MaxValue);

    /// <summary>Gets the red component of the color.</summary>
    /// <value />
    public byte R => (byte)(this._colorValue >> 16 & (uint) byte.MaxValue);

    /// <summary>Gets the green component of the color.</summary>
    /// <value />
    public byte G => (byte)(this._colorValue >> 8 & (uint) byte.MaxValue);

    /// <summary>Gets the blue component of the color.</summary>
    /// <value />
    public byte B => (byte)(this._colorValue & (uint) byte.MaxValue);

    /// <summary>Gets the hue value.</summary>
    /// <value />
    public float Hue
    {
      get
      {
        float h;
        this.ToHsv(out h, out float _, out float _);
        return h;
      }
    }

    /// <param name="h">The hue value.</param>
    /// <param name="s">The saturation value.</param>
    /// <param name="l">The lightness/luminosity value.</param>
    /// <param name="a">The alpha value.</param>
    /// <summary>Creates a color from the specified hue, saturation, lightness/luminosity and alpha values.</summary>
    /// <returns>The new <see cref="T:SkiaSharp.Color" /> instance.</returns>
    public static Color FromHsl(float h, float s, float l, byte a = 255)
    {
      ColorF ColorF = ColorF.FromHsl(h, s, l);
      return new Color((byte) (ColorF.R * (float) byte.MaxValue), (byte) (ColorF.G * (float) byte.MaxValue), (byte) (ColorF.B * (float) byte.MaxValue), a);
    }

    /// <param name="h">The hue value.</param>
    /// <param name="s">The saturation value.</param>
    /// <param name="v">The value/brightness value.</param>
    /// <param name="a">The alpha value.</param>
    /// <summary>Creates a color from the specified hue, saturation, value/brightness and alpha values.</summary>
    /// <returns>The new <see cref="T:SkiaSharp.Color" /> instance.</returns>
    public static Color FromHsv(float h, float s, float v, byte a = 255)
    {
      ColorF ColorF = ColorF.FromHsv(h, s, v);
      return new Color((byte) (ColorF.R * (float) byte.MaxValue), (byte) (ColorF.G * (float) byte.MaxValue), (byte) (ColorF.B * (float) byte.MaxValue), a);
    }

    /// <param name="h">The hue value.</param>
    /// <param name="s">The saturation value.</param>
    /// <param name="l">The lightness/luminosity value.</param>
    /// <summary>Converts the current color into it's hue, saturation and lightness/luminosity values.</summary>
    /// <remarks>The alpha value is separate from the HSL calculation and will always be the same as <see cref="P:SkiaSharp.Color.Alpha" />.</remarks>
    public void ToHsl(out float h, out float s, out float l) => new ColorF((float) this.R / (float) byte.MaxValue, (float) this.G / (float) byte.MaxValue, (float) this.B / (float) byte.MaxValue).ToHsl(out h, out s, out l);

    /// <param name="h">The hue value.</param>
    /// <param name="s">The saturation value.</param>
    /// <param name="v">The value/brightness value.</param>
    /// <summary>Converts the current color into it's hue, saturation and value/brightness values.</summary>
    /// <remarks>The alpha value is separate from the HSV/HSB calculation and will always be the same as <see cref="P:SkiaSharp.Color.Alpha" />.</remarks>
    public void ToHsv(out float h, out float s, out float v) => new ColorF((float) this.R / (float) byte.MaxValue, (float) this.G / (float) byte.MaxValue, (float) this.B / (float) byte.MaxValue).ToHsv(out h, out s, out v);

    /// <summary>Returns the color as a string in the format: #AARRGGBB.</summary>
    /// <returns />
    /// <remarks />
    public override string ToString() =>
        $"#{(object)this.A:x2}{(object)this.R:x2}{(object)this.G:x2}{(object)this.B:x2}";

    /// <param name="obj">The color to compare with the current color.</param>
    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <returns>Returns <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    public bool Equals(Color obj) => (int) obj._colorValue == (int) this._colorValue;

    /// <param name="other">The object to compare with the current object.</param>
    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <returns>Returns <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object other) => other is Color Color && this.Equals(Color);

    /// <param name="left">The first color to compare.</param>
    /// <param name="right">The second color to compare.</param>
    /// <summary>Indicates whether two <see cref="T:SkiaSharp.Color" /> objects are equal.</summary>
    /// <returns>Returns <see langword="true" /> if <paramref name="left" /> is equal to <paramref name="right" />, otherwise <see langword="false" />.</returns>
    public static bool operator ==(Color left, Color right) => left.Equals(right);

    /// <param name="left">The first color to compare.</param>
    /// <param name="right">The second color to compare.</param>
    /// <summary>Indicates whether two <see cref="T:SkiaSharp.Color" /> objects are different.</summary>
    /// <returns>Returns <see langword="true" /> if <paramref name="left" /> is not equal to <paramref name="right" />, otherwise <see langword="false" />.</returns>
    public static bool operator !=(Color left, Color right) => !left.Equals(right);

    /// <summary>Serves as the default hash function.</summary>
    /// <returns>Returns a hash code for the current object.</returns>
    public override int GetHashCode() => this._colorValue.GetHashCode();

    /// <param name="color">The UInt32 representation of a color.</param>
    /// <summary>Converts a UInt32 to a <see cref="T:SkiaSharp.Color" />.</summary>
    /// <returns>The new <see cref="T:SkiaSharp.Color" /> instance.</returns>
    public static implicit operator Color(uint color) => new Color(color);

    /// <param name="color">The color to convert.</param>
    /// <summary>Converts a <see cref="T:SkiaSharp.Color" /> to a UInt32.</summary>
    /// <returns>The UInt32 value for the color.</returns>
    public static explicit operator uint(Color color) => color._colorValue;

    /// <param name="hexString">The hexadecimal string representation of a color.</param>
    /// <summary>Converts the hexadecimal string representation of a color to its <see cref="T:SkiaSharp.Color" /> equivalent.</summary>
    /// <returns>The new <see cref="T:SkiaSharp.Color" /> instance.</returns>
    /// <remarks>This method can parse a string in the forms with or without a preceding '#' character: AARRGGB, RRGGBB, ARGB, RGB.</remarks>
    public static Color Parse(string hexString)
    {
      Color color;
      if (!Color.TryParse(hexString, out color))
        throw new ArgumentException("Invalid hexadecimal color string.", nameof (hexString));
      return color;
    }

    /// <param name="hexString">The hexadecimal string representation of a color.</param>
    /// <param name="color">The new <see cref="T:SkiaSharp.Color" /> instance.</param>
    /// <summary>Converts the hexadecimal string representation of a color to its <see cref="T:SkiaSharp.Color" /> equivalent.</summary>
    /// <returns>Returns true if the conversion was successful, otherwise false.</returns>
    /// <remarks>This method can parse a string in the forms with or without a preceding '#' character: AARRGGB, RRGGBB, ARGB, RGB.</remarks>
    public static bool TryParse(string hexString, out Color color)
    {
      if (string.IsNullOrWhiteSpace(hexString))
      {
        color = Color.Empty;
        return false;
      }
      hexString = hexString.Trim().ToUpperInvariant();
      if (hexString[0] == '#')
        hexString = hexString.Substring(1);
      int length = hexString.Length;
      switch (length)
      {
        case 3:
        case 4:
          byte result1;
          if (length == 4)
          {
            if (!byte.TryParse(hexString[length - 4].ToString() + (object) hexString[length - 4], NumberStyles.HexNumber, (IFormatProvider)CultureInfo.InvariantCulture, out result1))
            {
              color = Color.Empty;
              return false;
            }
          }
          else
          {
              result1 = byte.MaxValue;
          }

          if (!byte.TryParse(hexString[length - 3].ToString() + (object)hexString[length - 3], NumberStyles.HexNumber, (IFormatProvider)CultureInfo.InvariantCulture, out var result2) || !byte.TryParse(hexString[length - 2].ToString() + (object) hexString[length - 2], NumberStyles.HexNumber, (IFormatProvider) CultureInfo.InvariantCulture, out var result3) || !byte.TryParse(hexString[length - 1].ToString() + (object) hexString[length - 1], NumberStyles.HexNumber, (IFormatProvider) CultureInfo.InvariantCulture, out var result4))
          {
            color = Color.Empty;
            return false;
          }
          color = new Color(result2, result3, result4, result1);
          return true;
        case 6:
        case 8: if (!uint.TryParse(hexString, NumberStyles.HexNumber, (IFormatProvider)CultureInfo.InvariantCulture, out var result5))
          {
            color = Color.Empty;
            return false;
          }
          color = (Color)result5;
          if (length == 6)
            color = color.WithAlpha(byte.MaxValue);
          return true;
        default:
          color = Empty;
          return false;
      }
    }

    /// <summary>
    ///     Returns hex string representation of the color.
    /// </summary>
    /// <returns>Color string in format: AARRGGBB</returns>
    public string? ToHex()
    {
        return this == Empty ? null : $"{this._colorValue:X8}";
    }
  }
}
