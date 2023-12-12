using System.Drawing;
using System.Text;

namespace PixiEditor.Models.IO.PaletteParsers.Support;

// From Swatch Buckler palette library
// Copyright © 2014-2015 Warren Galyen.
// https://github.com/warrengalyen/SwatchBuckler

// Licensed under the MIT License.

[Serializable]
public struct CmykColor
{
    #region Constants

    public static readonly CmykColor Empty;

    #endregion

    #region Instance Fields

    private int _alpha;

    private double _black;

    private double _cyan;

    private bool _isEmpty;

    private double _magenta;

    private double _yellow;

    #endregion

    #region Static Constructors

    static CmykColor()
    {
        Empty = new CmykColor
        {
            IsEmpty = true
        };
    }

    #endregion

    #region Public Constructors

    public CmykColor(double c, double m, double y, double k)
        : this(255, c, m, y, k)
    { }

    public CmykColor(int alpha, double c, double m, double y, double k)
    {
        _cyan = Math.Min(1, c);
        _magenta = Math.Min(1, m);
        _yellow = Math.Min(1, y);
        _black = Math.Min(1, k);
        _alpha = alpha;
        _isEmpty = false;
    }

    public CmykColor(Color color)
    {
        double c = (double)(255 - color.R) / 255;
        double m = (double)(255 - color.G) / 255;
        double y = (double)(255 - color.B) / 255;

        double k = (double)Math.Min(c, Math.Min(m, y));
        if (k == 1.0)
        {
            _cyan = 0;
            _magenta = 0;
            _yellow = 0;
            _black = 1;
        }
        else
        {
            _cyan = (c - k) / (1 - k);
            _magenta = (m - k) / (1 - k);
            _yellow = (y - k) / (1 - k);
            _black = k;
        }
        _alpha = color.A;
        _isEmpty = false;
    }

    #endregion

    #region Operators

    public static bool operator ==(CmykColor a, CmykColor b)
    {
        return (a.Cyan == b.Cyan
                && a.Magenta == b.Magenta
                && a.Yellow == b.Yellow
                && a.Black == b.Black);
    }

    public static implicit operator CmykColor(Color color)
    {
        return new CmykColor(color);
    }

    public static implicit operator Color(CmykColor color)
    {
        return color.ToRgbColor();
    }

    public static bool operator !=(CmykColor a, CmykColor b)
    {
        return !(a == b);
    }

    #endregion

    #region Overridden Methods

    public override bool Equals(object obj)
    {
        bool result;

        if (obj is CmykColor)
        {
            CmykColor color;

            color = (CmykColor)obj;
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
        builder.Append("C=");
        builder.Append(this.Cyan);
        builder.Append(", M=");
        builder.Append(this.Magenta);
        builder.Append(", Y=");
        builder.Append(this.Yellow);
        builder.Append(", K=");
        builder.Append(this.Black);
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

    public double Cyan
    {
        get { return _cyan; }
        set { _cyan = Math.Min(1, Math.Max(0, value)); }
    }

    public bool IsEmpty
    {
        get { return _isEmpty; }
        internal set { _isEmpty = value; }
    }

    public double Magenta
    {
        get { return _magenta; }
        set { _magenta = Math.Min(1, Math.Max(0, value)); }
    }

    public double Yellow
    {
        get { return _yellow; }
        set { _yellow = Math.Min(1, Math.Max(0, value)); }
    }

    public double Black
    {
        get { return _black; }
        set { _black = Math.Min(1, Math.Max(0, value)); }
    }

    #endregion

    #region Public Members

    public Color ToRgbColor()
    {
        return this.ToRgbColor(this.A);
    }

    public Color ToRgbColor(int alpha)
    {
        int red = Convert.ToInt32((1.0 - this.Cyan) * (1.0 - this.Black) * 255.0);
        int green = Convert.ToInt32((1.0 - this.Magenta) * (1.0 - this.Black) * 255.0);
        int blue = Convert.ToInt32((1.0 - this.Yellow) * (1.0 - this.Black) * 255.0);

        return Color.FromArgb(alpha, red, green, blue);
    }

    #endregion
}
