using System;
using PixiEditor.DrawingApi.Core.Bridge;

namespace PixiEditor.DrawingApi.Core.Numerics;

/// <summary>A 3x3 transformation matrix with perspective.</summary>
/// <remarks>
///     It extends the traditional 2D affine transformation matrix with three perspective components that allow simple
///     3D effects to be created with it. Those components must be manually set by using the
///     <see cref="Matrix3X3.Persp0" />, <see cref="Matrix3X3.Persp1" />,
///     <see cref="Matrix3X3.Persp2" /> fields of the matrix.
/// </remarks>
public struct Matrix3X3 : IEquatable<Matrix3X3>
{
    public const float DegreesToRadians = 0.017453292f;

    public static readonly Matrix3X3 Identity = new() { ScaleX = 1f, ScaleY = 1f, Persp2 = 1f };
    
    public static readonly Matrix3X3 Empty = default;

    /// <summary>Gets or sets the scaling in the x-direction.</summary>
    /// <value />
    public float ScaleX { get; set; }

    /// <summary>Gets or sets the skew in the x-direction.</summary>
    /// <value />
    public float SkewX { get; set; }

    /// <summary>Get or sets the translation in the x-direction.</summary>
    /// <value />
    public float TransX { get; set; }

    /// <summary>Gets or sets the skew in the y-direction.</summary>
    /// <value />
    public float SkewY { get; set; }

    /// <summary>Gets or sets the scaling in the y-direction.</summary>
    /// <value />
    public float ScaleY { get; set; }

    /// <summary>Get or sets the translation in the y-direction.</summary>
    /// <value />
    public float TransY { get; set; }

    /// <summary>Gets or sets the x-perspective.</summary>
    /// <value />
    public float Persp0 { get; set; }

    /// <summary>Gets or sets the y-perspective.</summary>
    /// <value />
    public float Persp1 { get; set; }

    /// <summary>Gets or sets the z-perspective.</summary>
    /// <value />
    public float Persp2 { get; set; }

    public readonly bool Equals(Matrix3X3 obj)
    {
        return ScaleX == (double)obj.ScaleX && SkewX == (double)obj.SkewX &&
               TransX == (double)obj.TransX && SkewY == (double)obj.SkewY &&
               ScaleY == (double)obj.ScaleY && TransY == (double)obj.TransY &&
               Persp0 == (double)obj.Persp0 && Persp1 == (double)obj.Persp1 &&
               Persp2 == (double)obj.Persp2;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is Matrix3X3 Matrix3x3 && Equals(Matrix3x3);
    }

    public static bool operator ==(Matrix3X3 left, Matrix3X3 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Matrix3X3 left, Matrix3X3 right)
    {
        return !left.Equals(right);
    }

    public override readonly int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(ScaleX);
        hashCode.Add(SkewX);
        hashCode.Add(TransX);
        hashCode.Add(SkewY);
        hashCode.Add(ScaleY);
        hashCode.Add(TransY);
        hashCode.Add(Persp0);
        hashCode.Add(Persp1);
        hashCode.Add(Persp2);
        return hashCode.ToHashCode();
    }

    public Matrix3X3(float[] values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        ScaleX = values.Length == 9
            ? values[0]
            : throw new ArgumentException(string.Format("The matrix array must have a length of {0}.", 9),
                nameof(values));
        SkewX = values[1];
        TransX = values[2];
        SkewY = values[3];
        ScaleY = values[4];
        TransY = values[5];
        Persp0 = values[6];
        Persp1 = values[7];
        Persp2 = values[8];
    }

    public Matrix3X3(
        float scaleX,
        float skewX,
        float transX,
        float skewY,
        float scaleY,
        float transY,
        float persp0,
        float persp1,
        float persp2)
    {
        ScaleX = scaleX;
        SkewX = skewX;
        TransX = transX;
        SkewY = skewY;
        ScaleY = scaleY;
        TransY = transY;
        Persp0 = persp0;
        Persp1 = persp1;
        Persp2 = persp2;
    }
    
    public readonly bool IsIdentity => Equals(Identity);

    /// <summary>
    ///     Gets or sets the matrix as a flat array: [ScaleX, SkewX, TransX, SkewY, ScaleY, TransY, Persp0, Persp1,
    ///     Persp2].
    /// </summary>
    /// <value />
    public float[] Values
    {
        readonly get => new float[9] {ScaleX, SkewX, TransX, SkewY, ScaleY, TransY, Persp0, Persp1, Persp2};
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(Values));
            }

            ScaleX = value.Length == 9
                ? value[0]
                : throw new ArgumentException(string.Format("The matrix array must have a length of {0}.", 9),
                    nameof(Values));
            SkewX = value[1];
            TransX = value[2];
            SkewY = value[3];
            ScaleY = value[4];
            TransY = value[5];
            Persp0 = value[6];
            Persp1 = value[7];
            Persp2 = value[8];
        }
    }

    /// <param name="values">The array to populate.</param>
    /// <summary>Populates the specified array with the matrix values.</summary>
    /// <remarks>The result will be the same as <see cref="P:SkiaSharp.Matrix3x3.Values" />.</remarks>
    public readonly void GetValues(float[] values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        if (values.Length != 9)
        {
            throw new ArgumentException(string.Format("The matrix array must have a length of {0}.", 9),
                nameof(values));
        }

        values[0] = ScaleX;
        values[1] = SkewX;
        values[2] = TransX;
        values[3] = SkewY;
        values[4] = ScaleY;
        values[5] = TransY;
        values[6] = Persp0;
        values[7] = Persp1;
        values[8] = Persp2;
    }
    
    public VecD MapPoint(int p0, int p1)
    {
        return DrawingBackendApi.Current.MatrixImplementation.MapPoint(this, p0, p1);   
    }
    
    public VecD MapPoint(VecI size) => MapPoint(size.X, size.Y);

    public static Matrix3X3 CreateIdentity()
    {
        return new Matrix3X3() { ScaleX = 1f, ScaleY = 1f, Persp2 = 1f };
    }

    public static Matrix3X3 CreateTranslation(float x, float y)
    {
        if (x == 0.0 && y == 0.0)
        {
            return Identity;
        }

        return new Matrix3X3
        {
            ScaleX = 1f,
            ScaleY = 1f,
            TransX = x,
            TransY = y,
            Persp2 = 1f
        };
    }
    
    public static Matrix3X3 CreateScale(float x, float y)
    {
        if (x == 1.0 && y == 1.0)
        {
            return Identity;
        }

        return new Matrix3X3 {ScaleX = x, ScaleY = y, Persp2 = 1f};
    }
    
    public static Matrix3X3 CreateScale(float x, float y, float pivotX, float pivotY)
    {
        if (x == 1.0 && y == 1.0)
        {
            return Identity;
        }

        var num1 = pivotX - (x * pivotX);
        var num2 = pivotY - (y * pivotY);
        return new Matrix3X3
        {
            ScaleX = x,
            ScaleY = y,
            TransX = num1,
            TransY = num2,
            Persp2 = 1f
        };
    }
    
    public static Matrix3X3 CreateRotation(float radians)
    {
        if (radians == 0.0)
        {
            return Identity;
        }

        var sin = (float)Math.Sin(radians);
        var cos = (float)Math.Cos(radians);
        var identity = Identity;
        SetSinCos(ref identity, sin, cos);
        return identity;
    }
    
    public static Matrix3X3 CreateRotation(float radians, float pivotX, float pivotY)
    {
        if (radians == 0.0)
        {
            return Identity;
        }

        var sin = (float)Math.Sin(radians);
        var cos = (float)Math.Cos(radians);
        var identity = Identity;
        SetSinCos(ref identity, sin, cos, pivotX, pivotY);
        return identity;
    }


    public static Matrix3X3 CreateRotationDegrees(float degrees)
    {
        return degrees == 0.0 ? Identity : CreateRotation(degrees * ((float)Math.PI / 180f));
    }
    
    public static Matrix3X3 CreateRotationDegrees(float degrees, float pivotX, float pivotY)
    {
        return degrees == 0.0 ? Identity : CreateRotation(degrees * ((float)Math.PI / 180f), pivotX, pivotY);
    }
    
    public static Matrix3X3 CreateSkew(float x, float y)
    {
        if (x == 0.0 && y == 0.0)
        {
            return Identity;
        }

        return new Matrix3X3
        {
            ScaleX = 1f,
            SkewX = x,
            SkewY = y,
            ScaleY = 1f,
            Persp2 = 1f
        };
    }

    public static Matrix3X3 CreateScaleTranslation(float sx, float sy, float tx, float ty)
    {
        if (sx == 0.0 && sy == 0.0 && tx == 0.0 && ty == 0.0)
        {
            return Identity;
        }

        return new Matrix3X3
        {
            ScaleX = sx,
            SkewX = 0.0f,
            TransX = tx,
            SkewY = 0.0f,
            ScaleY = sy,
            TransY = ty,
            Persp0 = 0.0f,
            Persp1 = 0.0f,
            Persp2 = 1f
        };
    }

    public readonly bool IsInvertible => TryInvert(out _);

    /// <param name="inverse">The destination value to store the inverted matrix if the matrix can be inverted.</param>
    /// <summary>Attempts to invert the matrix, if possible the inverse matrix contains the result.</summary>
    /// <returns>
    ///     True if the matrix can be inverted, and the inverse parameter is initialized with the inverted matrix, false
    ///     otherwise.
    /// </returns>
    public readonly bool TryInvert(out Matrix3X3 inverse)
    {
        return DrawingBackendApi.Current.MatrixImplementation.TryInvert(this, out inverse);
    }
    
    public readonly Matrix3X3 Invert() => TryInvert(out var inverse) ? inverse : Matrix3X3.Empty;

    public static Matrix3X3 Concat(Matrix3X3 first, Matrix3X3 second)
    {
        return DrawingBackendApi.Current.MatrixImplementation.Concat(in first, in second);
    }
    
    public Matrix3X3 PostConcat(Matrix3X3 globalMatrix)
    {
        return DrawingBackendApi.Current.MatrixImplementation.PostConcat(in this, in globalMatrix);
    }

    private static void SetSinCos(ref Matrix3X3 matrix, float sin, float cos)
    {
        matrix.ScaleX = cos;
        matrix.SkewX = -sin;
        matrix.TransX = 0.0f;
        matrix.SkewY = sin;
        matrix.ScaleY = cos;
        matrix.TransY = 0.0f;
        matrix.Persp0 = 0.0f;
        matrix.Persp1 = 0.0f;
        matrix.Persp2 = 1f;
    }

    private static void SetSinCos(
        ref Matrix3X3 matrix,
        float sin,
        float cos,
        float pivotx,
        float pivoty)
    {
        var c = 1f - cos;
        matrix.ScaleX = cos;
        matrix.SkewX = -sin;
        matrix.TransX = Dot(sin, pivoty, c, pivotx);
        matrix.SkewY = sin;
        matrix.ScaleY = cos;
        matrix.TransY = Dot(-sin, pivotx, c, pivoty);
        matrix.Persp0 = 0.0f;
        matrix.Persp1 = 0.0f;
        matrix.Persp2 = 1f;
    }

    private static float Dot(float a, float b, float c, float d)
    {
        return (float)((a * (double)b) + (c * (double)d));
    }

    private static float Cross(float a, float b, float c, float d)
    {
        return (float)((a * (double)b) - (c * (double)d));
    }
    

    private class Indices
    {
        public const int ScaleX = 0;
        public const int SkewX = 1;
        public const int TransX = 2;
        public const int SkewY = 3;
        public const int ScaleY = 4;
        public const int TransY = 5;
        public const int Persp0 = 6;
        public const int Persp1 = 7;
        public const int Persp2 = 8;
        public const int Count = 9;
    }
}
