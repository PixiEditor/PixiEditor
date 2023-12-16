using System.Drawing;
using System.Text;

namespace PixiEditor.Models.IO.PaletteParsers.Support;

// Copyright © 2023 Warren Galyen.
// https://github.com/warrengalyen

// Licensed under the MIT License.

[Serializable]
public struct HsbColor
{
    #region Constants

    public static readonly HsbColor Empty;

    #endregion

    #region Instance Fields

    private int _alpha;

    private double _brightness;

    private double _hue;

    private bool _isEmpty;

    private double _saturation;

    #endregion

    #region Static Constructors

    static HsbColor()
    {
        Empty = new HsbColor
        {
            IsEmpty = true
        };
    }

    #endregion

    #region Public Constructors

    public HsbColor(double hue, double saturation, double brightness)
        : this(255, hue, saturation, brightness)
    { }

    public HsbColor(int alpha, double hue, double saturation, double brightness)
    {
        _hue = Math.Min(359, hue);
        _saturation = Math.Min(1, saturation);
        _brightness = Math.Min(1, brightness);
        _alpha = alpha;
        _isEmpty = false;
    }

    public HsbColor(Color color)
    {
        _alpha = color.A;
        _hue = color.GetHue();
        _saturation = color.GetSaturation();
        _brightness = color.GetBrightness();
        _isEmpty = false;
    }

    #endregion

    #region Operators

    public static bool operator ==(HsbColor a, HsbColor b)
    {
        return a.H == b.H && a.B == b.B && a.S == b.S && a.A == b.A;
    }

    public static implicit operator HsbColor(Color color)
    {
        return new HsbColor(color);
    }

    public static implicit operator Color(HsbColor color)
    {
        return color.ToRgbColor();
    }

    public static bool operator !=(HsbColor a, HsbColor b)
    {
        return !(a == b);
    }

    #endregion

    #region Overridden Methods

    public override bool Equals(object obj)
    {
        bool result;

        if (obj is HsbColor)
        {
            HsbColor color;

            color = (HsbColor)obj;
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
        builder.Append(", B=");
        builder.Append(B);
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

    public double B
    {
        get { return _brightness; }
        set { _brightness = Math.Min(1, Math.Max(0, value)); }
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
        double[] colors = new[] { 0.0, 0.0, 0.0 };

        if (S == 0)
        {
            // achromatic (gray)
            colors[0] = B;
            colors[1] = B;
            colors[2] = B;
        }
        else
        {
            // The color wheel consists of 6 sectors.
            // Figure out which sector you're in.
            double sectorPosition = H / 60;
            int sectorNumber = (int)Math.Floor(sectorPosition);

            // get the fractional part of the sector.
            // That is, how many degrees into the sector
            // are you?
            double fractionalSector = sectorPosition - sectorNumber;

            // Calculate values for the three axes
            // of the color.
            double p = B * (1 - S);
            double q = B * (1 - (S * fractionalSector));
            double t = B * (1 - (S * (1 - fractionalSector)));

            // Assign the fractional colors to r, g, and b
            // based on the sector the angle is in.
            switch (sectorNumber)
            {
                case 0:
                    colors[0] = B;
                    colors[1] = t;
                    colors[2] = p;
                    break;
                case 1:
                    colors[0] = q;
                    colors[1] = B;
                    colors[2] = p;
                    break;
                case 2:
                    colors[0] = p;
                    colors[1] = B;
                    colors[2] = t;
                    break;
                case 3:
                    colors[0] = p;
                    colors[1] = q;
                    colors[2] = B;
                    break;
                case 4:
                    colors[0] = t;
                    colors[1] = p;
                    colors[2] = B;
                    break;
                case 5:
                    colors[0] = B;
                    colors[1] = p;
                    colors[2] = q;
                    break;
            }
        }

        // return with values scaled to be between 0 and 255.
        return Color.FromArgb(alpha, (int)Math.Round(colors[0] * 255.0), (int)Math.Round(colors[1] * 255.0), (int)Math.Round((colors[2] * 255.0)));
    }

    #endregion
}
