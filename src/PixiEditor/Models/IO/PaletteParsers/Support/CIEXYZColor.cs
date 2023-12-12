using System.Drawing;
using System.Text;

namespace PixiEditor.Models.IO.PaletteParsers.Support;

// From Swatch Buckler palette library
// Copyright © 2014-2015 Warren Galyen.
// https://github.com/warrengalyen/SwatchBuckler

// Licensed under the MIT License.

[Serializable]
public struct CIEXYZColor
{
    #region Constants

    public static readonly CIEXYZColor Empty;

    // Gets the CIE D65 (white) structure.
    public static readonly CIEXYZColor D65 = new CIEXYZColor(0.9505, 1.0, 1.0890);

    #endregion

    #region Instance Fields

    private int _alpha;

    private bool _isEmpty;

    private double _x;

    private double _y;

    private double _z;

    #endregion

    #region Static Constructors

    static CIEXYZColor()
    {
        Empty = new CIEXYZColor
        {
            IsEmpty = true
        };
    }

    #endregion

    #region Public Constructors

    public CIEXYZColor(double x, double y, double z)
        : this(255, x, y, z)
    { }

    public CIEXYZColor(int alpha, double x, double y, double z)
    {
        _x = Math.Min(0.9505, x);
        _y = Math.Min(1, y);
        _z = Math.Min(1.089, z);
        _alpha = alpha;
        _isEmpty = false;
    }

    public CIEXYZColor(Color color)
    {
        double rLinear = (double)color.R / 255;
        double gLinear = (double)color.G / 255;
        double bLinear = (double)color.B / 255;

        double r = (rLinear > 0.04045) ? Math.Pow((rLinear + 0.055) / (1 + 0.055), 2.2) : (rLinear / 12.92);
        double g = (gLinear > 0.04045) ? Math.Pow((gLinear + 0.055) / (1 + 0.055), 2.2) : (gLinear / 12.92);
        double b = (bLinear > 0.04045) ? Math.Pow((bLinear + 0.055) / (1 + 0.055), 2.2) : (bLinear / 12.92);

        _x = (r * 0.4124 + g * 0.3576 + b * 0.1805);
        _y = (r * 0.2126 + g * 0.7152 + b * 0.0722);
        _z = (r * 0.0193 + g * 0.1192 + b * 0.9505);
        _alpha = color.A;
        _isEmpty = false;
    }

    #endregion

    #region Operators

    public static bool operator ==(CIEXYZColor a, CIEXYZColor b)
    {
        return (a.X == b.X
                && a.Y == b.Y
                && a.Z == b.Z);
    }

    public static implicit operator CIEXYZColor(Color color)
    {
        return new CIEXYZColor(color);
    }

    public static implicit operator Color(CIEXYZColor color)
    {
        return color.ToRgbColor();
    }

    public static bool operator !=(CIEXYZColor a, CIEXYZColor b)
    {
        return !(a == b);
    }

    #endregion

    #region Overridden Methods

    public override bool Equals(object obj)
    {
        bool result;

        if (obj is CIEXYZColor)
        {
            CIEXYZColor color;

            color = (CIEXYZColor)obj;
            result = (this == color);
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
        builder.Append(this.GetType().Name);
        builder.Append(" [");
        builder.Append("X=");
        builder.Append(this.X);
        builder.Append(", Y=");
        builder.Append(this.Y);
        builder.Append(", Z=");
        builder.Append(this.Z);
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

    public bool IsEmpty
    {
        get { return _isEmpty; }
        internal set { _isEmpty = value; }
    }

    public double X
    {
        get { return _x; }
        set { _x = Math.Min(0.9505, Math.Max(0, value)); }
    }


    public double Y
    {
        get { return _y; }
        set { _y = Math.Min(1, Math.Max(0, value)); }
    }

    public double Z
    {
        get { return _z; }
        set { _z = Math.Min(1.089, Math.Max(0, value)); }
    }

    #endregion

    #region Public Members

    public Color ToRgbColor()
    {
        return this.ToRgbColor(this.A);
    }

    public Color ToRgbColor(int alpha)
    {
        double[] linear = new double[3];
        linear[0] = this.X * 3.2410 - this.Y * 1.5374 - this.Z * 0.4986;  // red
        linear[1] = -this.X * 09692 + this.Y * 1.8760 - this.Z * 0.0416;  // green
        linear[2] = this.X * 0.0556 - this.Y * 0.2040 + this.Z * 1.0570;  // blue

        for (int i = 0; i < 3; i++)
        {
            linear[i] = (linear[i] <= 0.0031308) ? 12.92 * linear[i] : (1 + 0.055) * Math.Pow(linear[i], (1.0 / 2.4)) - 0.055;
        }

        return Color.FromArgb(alpha,
              Convert.ToInt32(Double.Parse(String.Format("{0:0.00}", linear[0] * 255.0))),
              Convert.ToInt32(Double.Parse(String.Format("{0:0.00}", linear[1] * 255.0))),
              Convert.ToInt32(Double.Parse(String.Format("{0:0.00}", linear[2] * 255.0))));
    }

    #endregion
}
