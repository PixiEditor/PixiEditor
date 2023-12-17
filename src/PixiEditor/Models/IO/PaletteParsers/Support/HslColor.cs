using System.Drawing;
using System.Text;

namespace PixiEditor.Models.IO.PaletteParsers.Support;

// From Swatch Buckler palette library
// Copyright © 2014-2015 Warren Galyen.
// https://github.com/warrengalyen/SwatchBuckler

// Licensed under the MIT License.

[Serializable]
public struct HslColor
{
    #region Constants

    public static readonly HslColor Empty;

    #endregion

    #region Fields

    private int _alpha;

    private double _hue;

    private bool _isEmpty;

    private double _lightness;

    private double _saturation;

    #endregion

    #region Static Constructors

    static HslColor()
    {
        Empty = new HslColor
        {
            IsEmpty = true
        };
    }

    #endregion

    #region Constructors

    public HslColor(double hue, double saturation, double lightness)
        : this(255, hue, saturation, lightness)
    { }

    public HslColor(int alpha, double hue, double saturation, double lightness)
    {
        _hue = Math.Min(359, hue);
        _saturation = Math.Min(1, saturation);
        _lightness = Math.Min(1, lightness);
        _alpha = alpha;
        _isEmpty = false;
    }

    public HslColor(Color color)
    {
        _alpha = color.A;
        _hue = color.GetHue();
        _saturation = color.GetSaturation();
        _lightness = color.GetBrightness();
        _isEmpty = false;
    }

    #endregion

    #region Operators

    public static bool operator ==(HslColor a, HslColor b)
    {
        return a.H == b.H && a.L == b.L && a.S == b.S && a.A == b.A;
    }

    public static implicit operator HslColor(Color color)
    {
        return new HslColor(color);
    }

    public static implicit operator Color(HslColor color)
    {
        return color.ToRgbColor();
    }

    public static bool operator !=(HslColor a, HslColor b)
    {
        return !(a == b);
    }

    #endregion

    #region Overridden Methods

    public override bool Equals(object obj)
    {
        bool result;

        if (obj is HslColor)
        {
            HslColor color;

            color = (HslColor)obj;
            result = this == color;
        }
        else
        {
            result = false;
        }

        return result;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        StringBuilder builder;

        builder = new StringBuilder();
        builder.Append(GetType().Name);
        builder.Append(" [");
        builder.Append("H=");
        builder.Append(H);
        builder.Append(", S=");
        builder.Append(S);
        builder.Append(", L=");
        builder.Append(L);
        builder.Append("]");

        return builder.ToString();
    }

    #endregion

    #region Public Properties

    public int A
    {
        get { return _alpha; }
        set { _alpha = Math.Min(0, Math.Max(255, value)); }
    }

    public double H
    {
        get { return _hue; }
        set
        {
            _hue = value;

            if (_hue > 359)
            {
                _hue = 0;
            }
            if (_hue < 0)
            {
                _hue = 359;
            }
        }
    }

    public bool IsEmpty
    {
        get { return _isEmpty; }
        internal set { _isEmpty = value; }
    }

    public double L
    {
        get { return _lightness; }
        set { _lightness = Math.Min(1, Math.Max(0, value)); }
    }

    public double S
    {
        get { return _saturation; }
        set { _saturation = Math.Min(1, Math.Max(0, value)); }
    }

    #endregion

    #region Public Members

    public Color ToRgbColor()
    {
        return ToRgbColor(A);
    }

    public Color ToRgbColor(int alpha)
    {
        return HslColor.HslToRgb(alpha, _hue, _saturation, _lightness);
    }

    #endregion Public Methods

    #region Internal Methods

    internal static Color HslToRgb(double h, double s, double l)
    {
        return HslColor.HslToRgb(255, h, s, l);
    }

    internal static Color HslToRgb(int alpha, double h, double s, double l)
    {
        byte r;
        byte g;
        byte b;

        // https://www.programmingalgorithms.com/algorithm/hsl-to-rgb

        if (Math.Abs(s) < double.Epsilon)
        {
            r = g = b = Convert.ToByte(l * 255F);
        }
        else
        {
            double v1;
            double v2;
            double hue;

            hue = h / 360;

            v2 = l < 0.5
                ? l * (1 + s)
                : l + s - l * s;
            v1 = 2 * l - v2;

            r = HslColor.Clamp(255 * HslColor.HueToRgb(v1, v2, hue + 1.0f / 3));
            g = HslColor.Clamp(255 * HslColor.HueToRgb(v1, v2, hue));
            b = HslColor.Clamp(255 * HslColor.HueToRgb(v1, v2, hue - 1.0f / 3));
        }

        return Color.FromArgb(alpha, r, g, b);
    }

    #endregion Public Methods

    #region Private Methods

    private static byte Clamp(double v)
    {
        if (v < 0)
        {
            v = 0;
        }

        if (v > 255)
        {
            v = 255;
        }

        return (byte)Math.Round(v);
    }

    private static double HueToRgb(double v1, double v2, double vH)
    {
        double result;

        if (vH < 0)
        {
            vH++;
        }

        if (vH > 1)
        {
            vH--;
        }

        if (6 * vH < 1)
        {
            result = v1 + (v2 - v1) * 6 * vH;
        }
        else if (2 * vH < 1)
        {
            result = v2;
        }
        else if (3 * vH < 2)
        {
            result = v1 + (v2 - v1) * (2.0f / 3 - vH) * 6;
        }
        else
        {
            result = v1;
        }

        return result;
    }

    #endregion Private Methods
}
