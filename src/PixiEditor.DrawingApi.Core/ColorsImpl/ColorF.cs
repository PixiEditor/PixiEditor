using System;
using PixiEditor.DrawingApi.Core.Bridge;

namespace PixiEditor.DrawingApi.Core.ColorsImpl
{
    public readonly struct ColorF : IEquatable<ColorF>
    {
        private const float Epsilon = 0.001f;

        /// <summary>Gets an "empty" color, with zero for all the components.</summary>
        public static readonly ColorF Empty;
        
        private readonly float _fR;
        private readonly float _fG;
        private readonly float _fB;
        private readonly float _fA;

        public ColorF(float r, float g, float b)
        {
            _fR = r;
            _fG = g;
            _fB = b;
            _fA = 1f;
        }

        public ColorF(float r, float g, float b, float a)
        {
            _fR = r;
            _fG = g;
            _fB = b;
            _fA = a;
        }

        /// <summary>Gets the hue value.</summary>
        /// <value />
        public float Hue
        {
            get
            {
                float h;
                ToHsv(out h, out var _, out var _);
                return h;
            }
        }

        /// <summary>Gets the red component of the color.</summary>
        /// <value />
        public float R => _fR;

        /// <summary>Gets the green component of the color.</summary>
        /// <value />

        public float G => _fG;

        /// <summary>Gets the blue component of the color.</summary>
        /// <value />

        public float B => _fB;

        /// <summary>Gets the alpha component of the color.</summary>
        /// <value />

        public float A => _fA;

        /// <param name="obj">The color to compare with the current color.</param>
        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>
        ///     Returns <see langword="true" /> if the specified object is equal to the current object; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        /// 
        public bool Equals(ColorF obj)
        {
            return R == (double)obj._fR && G == (double)obj._fG && B == (double)obj._fB &&
                   A == (double)obj._fA;
        }

        /// <param name="red">The new red component.</param>
        /// <summary>Returns a new color based on this current instance, but with the new red channel value.</summary>
        /// <returns />
        public ColorF WithRed(float red)
        {
            return new ColorF(red, G, B, A);
        }

        /// <param name="green">The new green component.</param>
        /// <summary>Returns a new color based on this current instance, but with the new green channel value.</summary>
        /// <returns />
        public ColorF WithGreen(float green)
        {
            return new ColorF(R, green, B, A);
        }

        /// <param name="blue">The new blue component.</param>
        /// <summary>Returns a new color based on this current instance, but with the new blue channel value.</summary>
        public ColorF WithBlue(float blue)
        {
            return new ColorF(R, G, blue, A);
        }

        /// <param name="alpha">The new alpha component.</param>
        /// <summary>Returns a new color based on this current instance, but with the new alpha channel value.</summary>
        public ColorF WithAlpha(float alpha)
        {
            return new ColorF(R, G, B, alpha);
        }

        /// <summary>Clamp the color components in the range [0..1].</summary>
        /// <returns>Returns the clamped color.</returns>
        public ColorF Clamp()
        {
            return new ColorF(Clamp(R), Clamp(G), Clamp(B), Clamp(A));

            static float Clamp(float v)
            {
                if (v > 1.0)
                {
                    return 1f;
                }

                return v < 0.0 ? 0.0f : v;
            }
        }

        /// <param name="h">The hue value.</param>
        /// <param name="s">The saturation value.</param>
        /// <param name="l">The lightness/luminosity value.</param>
        /// <param name="a">The alpha value.</param>
        /// <summary>Creates a color from the specified hue, saturation, lightness/luminosity and alpha values.</summary>
        /// <returns>The new <see cref="T:DrawingApiCore.ColorF" /> instance.</returns>
        public static ColorF FromHsl(float h, float s, float l, float a = 1f)
        {
            h /= 360f;
            s /= 100f;
            l /= 100f;
            var red = l;
            var green = l;
            var blue = l;
            if (Math.Abs(s) > 1.0 / 1000.0)
            {
                var v2 = l >= 0.5 ? (float)(l + (double)s - (s * (double)l)) : l * (1f + s);
                var v1 = (2f * l) - v2;
                red = HueToRgb(v1, v2, h + 0.33333334f);
                green = HueToRgb(v1, v2, h);
                blue = HueToRgb(v1, v2, h - 0.33333334f);
            }

            return new ColorF(red, green, blue, a);
        }

        private static float HueToRgb(float v1, float v2, float vH)
        {
            if (vH < 0.0)
            {
                ++vH;
            }

            if (vH > 1.0)
            {
                --vH;
            }

            if (6.0 * vH < 1.0)
            {
                return v1 + ((float)((v2 - (double)v1) * 6.0) * vH);
            }

            if (2.0 * vH < 1.0)
            {
                return v2;
            }

            return 3.0 * vH < 2.0 ? v1 + (float)((v2 - (double)v1) * (0.6666666865348816 - vH) * 6.0) : v1;
        }

        /// <param name="h">The hue value.</param>
        /// <param name="s">The saturation value.</param>
        /// <param name="v">The value/brightness value.</param>
        /// <param name="a">The alpha value.</param>
        /// <summary>Creates a color from the specified hue, saturation, value/brightness and alpha values.</summary>
        /// <returns>The new <see cref="T:DrawingApiCore.ColorF" /> instance.</returns>
        public static ColorF FromHsv(float h, float s, float v, float a = 1f)
        {
            h /= 360f;
            s /= 100f;
            v /= 100f;
            var red = v;
            var green = v;
            var blue = v;
            if (Math.Abs(s) > 1.0 / 1000.0)
            {
                h *= 6f;
                if (Math.Abs(h - 6f) < 1.0 / 1000.0)
                {
                    h = 0.0f;
                }

                var num1 = (int)h;
                var num2 = v * (1f - s);
                var num3 = v * (float)(1.0 - (s * (h - (double)num1)));
                var num4 = v * (float)(1.0 - (s * (1.0 - (h - (double)num1))));
                switch (num1)
                {
                    case 0:
                        red = v;
                        green = num4;
                        blue = num2;
                        break;
                    case 1:
                        red = num3;
                        green = v;
                        blue = num2;
                        break;
                    case 2:
                        red = num2;
                        green = v;
                        blue = num4;
                        break;
                    case 3:
                        red = num2;
                        green = num3;
                        blue = v;
                        break;
                    case 4:
                        red = num4;
                        green = num2;
                        blue = v;
                        break;
                    default:
                        red = v;
                        green = num2;
                        blue = num3;
                        break;
                }
            }

            return new ColorF(red, green, blue, a);
        }

        /// <param name="h">The hue value.</param>
        /// <param name="s">The saturation value.</param>
        /// <param name="l">The lightness/luminosity value.</param>
        /// <summary>Converts the current color into it's hue, saturation and lightness/luminosity values.</summary>
        /// <remarks>
        ///     The alpha value is separate from the HSL calculation and will always be the same as
        ///     <see cref="P:DrawingApiCore.ColorF.Alpha" />.
        /// </remarks>
        public void ToHsl(out float h, out float s, out float l)
        {
            var fR = R;
            var fG = G;
            var fB = B;
            var num1 = Math.Min(Math.Min(fR, fG), fB);
            var num2 = Math.Max(Math.Max(fR, fG), fB);
            var num3 = num2 - num1;
            h = 0.0f;
            s = 0.0f;
            l = (float)((num2 + (double)num1) / 2.0);
            if (Math.Abs(num3) > 1.0 / 1000.0)
            {
                s = l >= 0.5 ? num3 / (2f - num2 - num1) : num3 / (num2 + num1);
                var num4 = (float)(((num2 - (double)fR) / 6.0) + (num3 / 2.0)) / num3;
                var num5 = (float)(((num2 - (double)fG) / 6.0) + (num3 / 2.0)) / num3;
                var num6 = (float)(((num2 - (double)fB) / 6.0) + (num3 / 2.0)) / num3;
                h = Math.Abs(fR - num2) >= 1.0 / 1000.0
                    ? Math.Abs(fG - num2) >= 1.0 / 1000.0 ? 0.6666667f + num5 - num4 : 0.33333334f + num4 - num6
                    : num6 - num5;
                if (h < 0.0)
                {
                    ++h;
                }

                if (h > 1.0)
                {
                    --h;
                }
            }

            h *= 360f;
            s *= 100f;
            l *= 100f;
        }

        /// <param name="h">The hue value.</param>
        /// <param name="s">The saturation value.</param>
        /// <param name="v">The value/brightness value.</param>
        /// <summary>Converts the current color into it's hue, saturation and value/brightness values.</summary>
        /// <remarks>
        ///     The alpha value is separate from the HSV/HSB calculation and will always be the same as
        ///     <see cref="P:DrawingApiCore.ColorF.Alpha" />.
        /// </remarks>
        public void ToHsv(out float h, out float s, out float v)
        {
            var fR = R;
            var fG = G;
            var fB = B;
            var num1 = Math.Min(Math.Min(fR, fG), fB);
            var num2 = Math.Max(Math.Max(fR, fG), fB);
            var num3 = num2 - num1;
            h = 0.0f;
            s = 0.0f;
            v = num2;
            if (Math.Abs(num3) > 1.0 / 1000.0)
            {
                s = num3 / num2;
                var num4 = (float)(((num2 - (double)fR) / 6.0) + (num3 / 2.0)) / num3;
                var num5 = (float)(((num2 - (double)fG) / 6.0) + (num3 / 2.0)) / num3;
                var num6 = (float)(((num2 - (double)fB) / 6.0) + (num3 / 2.0)) / num3;
                h = Math.Abs(fR - num2) >= 1.0 / 1000.0
                    ? Math.Abs(fG - num2) >= 1.0 / 1000.0 ? 0.6666667f + num5 - num4 : 0.33333334f + num4 - num6
                    : num6 - num5;
                if (h < 0.0)
                {
                    ++h;
                }

                if (h > 1.0)
                {
                    --h;
                }
            }

            h *= 360f;
            s *= 100f;
            v *= 100f;
        }

        /// <summary>Returns the color as a string in the format: #AARRGGBB.</summary>
        /// <remarks>As a result of converting a floating-point color to an integer color, some data loss will occur.</remarks>
        public override string ToString()
        {
            return ((Color)this).ToString();
        }

        /// <param name="color">The <see cref="T:DrawingApiCore.SKColor" />.</param>
        /// <summary>Converts a <see cref="T:DrawingApiCore.SKColor" /> to a <see cref="T:DrawingApiCore.ColorF" />.</summary>
        /// <returns>The new <see cref="T:DrawingApiCore.ColorF" /> instance.</returns>
        public static implicit operator ColorF(Color color)
        {
            return DrawingBackendApi.Current.ColorImplementation.ColorToColorF((uint)color);
        }

        /// <param name="color">The color to convert.</param>
        /// <summary>Converts a <see cref="T:DrawingApiCore.ColorF" /> to a <see cref="T:DrawingApiCore.SKColor" />.</summary>
        /// <returns>The <see cref="T:DrawingApiCore.SKColor" />.</returns>
        /// <remarks>As a result of converting a floating-point color to an integer color, some data loss will occur.</remarks>
        public static explicit operator Color(ColorF color)
        {
            return DrawingBackendApi.Current.ColorImplementation.ColorFToColor(color);
        }

        /// <param name="obj">The object to compare with the current object.</param>
        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>
        ///     Returns <see langword="true" /> if the specified object is equal to the current object; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is ColorF colorF && Equals(colorF);
        }

        /// <param name="left">The first color to compare.</param>
        /// <param name="right">The second color to compare.</param>
        /// <summary>Indicates whether two <see cref="T:DrawingApiCore.ColorF" /> objects are equal.</summary>
        /// <returns>
        ///     Returns <see langword="true" /> if <paramref name="left" /> is equal to <paramref name="right" />, otherwise
        ///     <see langword="false" />.
        /// </returns>
        public static bool operator ==(ColorF left, ColorF right)
        {
            return left.Equals(right);
        }

        /// <param name="left">The first color to compare.</param>
        /// <param name="right">The second color to compare.</param>
        /// <summary>Indicates whether two <see cref="T:DrawingApiCore.ColorF" /> objects are different.</summary>
        /// <returns>
        ///     Returns <see langword="true" /> if <paramref name="left" /> is not equal to <paramref name="right" />,
        ///     otherwise <see langword="false" />.
        /// </returns>
        public static bool operator !=(ColorF left, ColorF right)
        {
            return !left.Equals(right);
        }

        /// <summary>Serves as the default hash function.</summary>
        /// <returns>Returns a hash code for the current object.</returns>
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(R);
            hashCode.Add(G);
            hashCode.Add(B);
            hashCode.Add(A);
            return hashCode.ToHashCode();
        }
    }
}

