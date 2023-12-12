using System.Drawing;
using System.Text;

namespace PixiEditor.Models.IO.PaletteParsers.Support;

// From Swatch Buckler palette library
// Copyright © 2014-2015 Warren Galyen.
// https://github.com/warrengalyen/SwatchBuckler

// Licensed under the MIT License.

// http://en.wikipedia.org/wiki/HSL_color_space

[Serializable]
public struct CIELabColor
{
    #region Constants

    public static readonly CIELabColor Empty;

    #endregion

    #region Instance Fields

    private int _alpha;

    private double _a;

    private double _b;

    private bool _isEmpty;

    private double _L;

    #endregion

    #region Static Constructors

    static CIELabColor()
    {
        Empty = new CIELabColor
        {
            IsEmpty = true
        };
    }

    #endregion

    #region Public Constructors

    public CIELabColor(double l, double a, double b)
        : this(255, l, a, b)
    { }

    public CIELabColor(int alpha, double l, double a, double b)
    {
        _L = Math.Min(1, l);
        _a = Math.Min(1, a);
        _b = Math.Min(1, b);
        _alpha = alpha;
        _isEmpty = false;
    }

    public CIELabColor(Color color)
    {
        // perform RGB to XYZ conversion
        CIEXYZColor xyz = new CIEXYZColor(color);

        // convert XYZ to LAB
        _L = 116.0 * Fxyz(xyz.Y / CIEXYZColor.D65.Y) - 16;
        _a = 500.0 * (Fxyz(xyz.X / CIEXYZColor.D65.X) - Fxyz(xyz.Y / CIEXYZColor.D65.Y));
        _b = 200.0 * (Fxyz(xyz.Y / CIEXYZColor.D65.Y) - Fxyz(xyz.Z / CIEXYZColor.D65.Z));
        _alpha = color.A;
        _isEmpty = false;
    }

    #endregion

    #region Operators

    public static bool operator ==(CIELabColor a, CIELabColor b)
    {
        return (a.L == b.L
                && a.A == b.A
                && a.B == b.B);
    }

    public static implicit operator CIELabColor(Color color)
    {
        return new CIELabColor(color);
    }

    public static implicit operator Color(CIELabColor color)
    {
        return color.ToRgbColor();
    }

    public static bool operator !=(CIELabColor a, CIELabColor b)
    {
        return !(a == b);
    }

    #endregion

    #region Overridden Methods

    public override bool Equals(object obj)
    {
        bool result;

        if (obj is CIELabColor)
        {
            CIELabColor color;

            color = (CIELabColor)obj;
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
        builder.Append("L=");
        builder.Append(this.L);
        builder.Append(", A=");
        builder.Append(this.A);
        builder.Append(", B=");
        builder.Append(this.B);
        builder.Append("]");

        return builder.ToString();
    }

    #endregion

    #region Public Properties

    public int Alpha
    {
        get { return _alpha; }
        set { _alpha = Math.Min(0, Math.Max(255, value)); }
    }

    public double L
    {
        get { return _L; }
        set { _L = Math.Min(1, Math.Max(0, value)); }
    }

    public bool IsEmpty
    {
        get { return _isEmpty; }
        internal set { _isEmpty = value; }
    }

    public double A
    {
        get { return _a; }
        set { _a = Math.Min(1, Math.Max(0, value)); }
    }

    public double B
    {
        get { return _b; }
        set { _b = Math.Min(1, Math.Max(0, value)); }
    }

    #endregion

    #region Public Members

    public Color ToRgbColor()
    {
        return this.ToRgbColor(this.Alpha);
    }

    public Color ToRgbColor(int alpha)
    {
        // perform LAB to XYZ conversion
        double theta = 6.0 / 29.0;

        double fy = (this.L + 16) / 116.0;
        double fx = fy + (this.A / 500.0);
        double fz = fy - (this.B / 200.0);

        CIEXYZColor xyz = new CIEXYZColor(
            (fx > theta) ? CIEXYZColor.D65.X * (fx * fx * fx) : (fx - 16.0 / 1160.0) * 3 * (theta * theta) * CIEXYZColor.D65.X,
            (fy > theta) ? CIEXYZColor.D65.Y * (fy * fy * fy) : (fy - 16.0 / 1160.0) * 3 * (theta * theta) * CIEXYZColor.D65.Y,
            (fz > theta) ? CIEXYZColor.D65.Z * (fz * fz * fz) : (fz - 16.0 / 1160.0) * 3 * (theta * theta) * CIEXYZColor.D65.Z);

        // perform XYZ to LAB conversion
        double[] linear = new double[3];
        linear[0] = xyz.X * 3.2410 - xyz.Y * 1.5374 - xyz.Z * 0.4986;  // red
        linear[1] = -xyz.X * 0.9692 + xyz.Y * 1.8760 - xyz.Z * 0.0416; // green
        linear[2] = xyz.X * 0.0556 - xyz.Y * 0.2040 + xyz.Z * 1.0570;  // blue

        return Color.FromArgb(alpha,
            Convert.ToInt32(Double.Parse(String.Format("{0:0.00}", linear[0] * 255.0))),
            Convert.ToInt32(Double.Parse(String.Format("{0:0.00}", linear[1] * 255.0))),
            Convert.ToInt32(Double.Parse(String.Format("{0:0.00}", linear[2] * 255.0))));
    }

    #endregion

    #region Private Members

    /// <summary>
    /// XYZ to L*a*b* transformation function.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private static double Fxyz(double t)
    {
        return ((t > 0.008856) ? Math.Pow(t, (1.0 / 3.0)) : (7.787 * t + 16.0 / 116.0));
    }

    #endregion
}
