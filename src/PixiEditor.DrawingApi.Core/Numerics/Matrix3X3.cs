using System;
using System.ComponentModel;
using PixiEditor.DrawingApi.Core.Bridge;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Core.Numerics;

/// <summary>A 3x3 transformation matrix with perspective.</summary>
/// <remarks>
///     It extends the traditional 2D affine transformation matrix with three perspective components that allow simple
///     3D effects to be created with it. Those components must be manually set by using the
///     <see cref="P:SkiaSharp.Matrix3x3.Persp0" />, <see cref="P:SkiaSharp.Matrix3x3.Persp1" />,
///     <see cref="P:SkiaSharp.Matrix3x3.Persp2" /> fields of the matrix.
/// </remarks>
public struct Matrix3X3 : IEquatable<Matrix3X3>
{
    internal const float DegreesToRadians = 0.017453292f;

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

    public static Matrix3X3 CreateIdentity()
    {
        return new Matrix3X3() {ScaleX = 1f, ScaleY = 1f, Persp2 = 1f};
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
    
    public readonly unsafe Matrix3X3 PreConcat(Matrix3X3 matrix)
    {
        var Matrix3x3 = this;
        SkiaApi.sk_matrix_pre_concat(&Matrix3x3, &matrix);
        return Matrix3x3;
    }
    
    public readonly unsafe Matrix3X3 PostConcat(Matrix3X3 matrix)
    {
        var Matrix3x3 = this;
        SkiaApi.sk_matrix_post_concat(&Matrix3x3, &matrix);
        return Matrix3x3;
    }

    /// <param name="target">The result matrix value.</param>
    /// <param name="first">The first matrix to concatenate.</param>
    /// <param name="second">The second matrix to concatenate.</param>
    /// <summary>Concatenates the specified matrices into the resulting target matrix.</summary>
    /// <remarks>Either source matrices can also be the target matrix.</remarks>
    public static unsafe void Concat(ref Matrix3X3 target, Matrix3X3 first, Matrix3X3 second)
    {
        fixed (Matrix3X3* result = &target)
        {
            SkiaApi.sk_matrix_concat(result, &first, &second);
        }
    }

    /// <param name="target">The result matrix value.</param>
    /// <param name="first">The first matrix to concatenate.</param>
    /// <param name="second">The second matrix to concatenate.</param>
    /// <summary>Concatenates the specified matrices into the resulting target matrix.</summary>
    /// <remarks>Either source matrices can also be the target matrix.</remarks>
    public static unsafe void Concat(ref Matrix3X3 target, ref Matrix3X3 first, ref Matrix3X3 second)
    {
        fixed (Matrix3X3* result = &target)
        fixed (Matrix3X3* first1 = &first)
        fixed (Matrix3X3* second1 = &second)
        {
            SkiaApi.sk_matrix_concat(result, first1, second1);
        }
    }

    /// <param name="target">The target matrix.</param>
    /// <param name="matrix">The matrix to be post-concatenated.</param>
    /// <summary>Pre-concatenates the matrix to the target matrix.</summary>
    /// <remarks>This represents: result = target * matrix</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use PreConcat(Matrix3x3) instead.")]
    public static unsafe void PreConcat(ref Matrix3X3 target, Matrix3X3 matrix)
    {
        fixed (Matrix3X3* result = &target)
        {
            SkiaApi.sk_matrix_pre_concat(result, &matrix);
        }
    }

    /// <param name="target">The target matrix.</param>
    /// <param name="matrix">The matrix to be post-concatenated.</param>
    /// <summary>Pre-concatenates the matrix to the target matrix.</summary>
    /// <remarks>This represents: result = target * matrix</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use PreConcat(Matrix3x3) instead.")]
    public static unsafe void PreConcat(ref Matrix3X3 target, ref Matrix3X3 matrix)
    {
        fixed (Matrix3X3* result = &target)
        fixed (Matrix3X3* matrix1 = &matrix)
        {
            SkiaApi.sk_matrix_pre_concat(result, matrix1);
        }
    }

    /// <param name="target">The target matrix.</param>
    /// <param name="matrix">The matrix to be post-concatenated.</param>
    /// <summary>Post-concatenates the matrix to the target matrix.</summary>
    /// <remarks>This represents: result = matrix * target</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use PostConcat(Matrix3x3) instead.")]
    public static unsafe void PostConcat(ref Matrix3X3 target, Matrix3X3 matrix)
    {
        fixed (Matrix3X3* result = &target)
        {
            SkiaApi.sk_matrix_post_concat(result, &matrix);
        }
    }

    /// <param name="target">The target matrix.</param>
    /// <param name="matrix">The matrix to be post-concatenated.</param>
    /// <summary>Post-concatenates the matrix to the target matrix.</summary>
    /// <remarks>This represents: result = matrix * target</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use PostConcat(Matrix3x3) instead.")]
    public static unsafe void PostConcat(ref Matrix3X3 target, ref Matrix3X3 matrix)
    {
        fixed (Matrix3X3* result = &target)
        fixed (Matrix3X3* matrix1 = &matrix)
        {
            SkiaApi.sk_matrix_post_concat(result, matrix1);
        }
    }

    /// <param name="source">The source rectangle to map.</param>
    /// <summary>Applies the matrix to a rectangle.</summary>
    /// <returns>Returns the mapped rectangle.</returns>
    /// <remarks />
    public readonly unsafe SKRect MapRect(SKRect source)
    {
        SKRect skRect;
        fixed (Matrix3X3* matrix = &this)
        {
            SkiaApi.sk_matrix_map_rect(matrix, &skRect, &source);
        }

        return skRect;
    }

    /// <param name="matrix">The transformation matrix.</param>
    /// <param name="dest">The mapped rectangle.</param>
    /// <param name="source">The source rectangle to map.</param>
    /// <summary>Applies the matrix to a rectangle.</summary>
    /// <remarks />
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use MapRect(SKRect) instead.")]
    public static unsafe void MapRect(ref Matrix3X3 matrix, out SKRect dest, ref SKRect source)
    {
        fixed (Matrix3X3* matrix1 = &matrix)
        fixed (SKRect* dest1 = &dest)
        fixed (SKRect* source1 = &source)
        {
            SkiaApi.sk_matrix_map_rect(matrix1, dest1, source1);
        }
    }

    /// <param name="point">The point to map.</param>
    /// <summary>Applies the matrix to a point.</summary>
    /// <returns>Returns the mapped point.</returns>
    /// <remarks>
    ///     Mapping points uses all components of the matrix. Use
    ///     <see cref="M:SkiaSharp.Matrix3x3.MapVector(System.Single,System.Single)" /> to ignore the translation.
    /// </remarks>
    public readonly SKPoint MapPoint(SKPoint point)
    {
        return MapPoint(point.X, point.Y);
    }

    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <summary>Applies the matrix to a point.</summary>
    /// <returns>Returns the mapped point.</returns>
    /// <remarks>
    ///     Mapping points uses all components of the matrix. Use
    ///     <see cref="M:SkiaSharp.Matrix3x3.MapVector(System.Single,System.Single)" /> to ignore the translation.
    /// </remarks>
    public readonly unsafe SKPoint MapPoint(float x, float y)
    {
        SKPoint skPoint;
        fixed (Matrix3X3* matrix = &this)
        {
            SkiaApi.sk_matrix_map_xy(matrix, x, y, &skPoint);
        }

        return skPoint;
    }

    /// <param name="result">
    ///     The array where the mapped results will be stored (needs to have the same number of elements of
    ///     the <paramref name="points" /> array).
    /// </param>
    /// <param name="points">The array of points to be mapped.</param>
    /// <summary>Applies the matrix to an array of points.</summary>
    /// <remarks>
    ///     Mapping points uses all components of the matrix. Use
    ///     <see cref="M:SkiaSharp.Matrix3x3.MapVectors(SkiaSharp.SKPoint[],SkiaSharp.SKPoint[])" /> to ignore the translation.
    /// </remarks>
    public readonly unsafe void MapPoints(SKPoint[] result, SKPoint[] points)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (points == null)
        {
            throw new ArgumentNullException(nameof(points));
        }

        if (result.Length != points.Length)
        {
            throw new ArgumentException("Buffers must be the same size.");
        }

        fixed (Matrix3X3* matrix = &this)
        fixed (SKPoint* dst = result)
        fixed (SKPoint* src = points)
        {
            SkiaApi.sk_matrix_map_points(matrix, dst, src, result.Length);
        }
    }

    /// <param name="points">The array of points to be mapped.</param>
    /// <summary>Applies the matrix to an array of points.</summary>
    /// <returns>Returns the new array allocated with the mapped results.</returns>
    /// <remarks>
    ///     Mapping points uses all components of the matrix. Use
    ///     <see cref="M:SkiaSharp.Matrix3x3.MapVectors(SkiaSharp.SKPoint[])" /> to ignore the translation.
    /// </remarks>
    public readonly SKPoint[] MapPoints(SKPoint[] points)
    {
        var result = points != null ? new SKPoint[points.Length] : throw new ArgumentNullException(nameof(points));
        MapPoints(result, points);
        return result;
    }

    /// <param name="vector">To be added.</param>
    /// <summary>To be added.</summary>
    /// <returns>To be added.</returns>
    /// <remarks>To be added.</remarks>
    public readonly SKPoint MapVector(SKPoint vector)
    {
        return MapVector(vector.X, vector.Y);
    }

    /// <param name="x">The x-component of the vector.</param>
    /// <param name="y">The y-component of the vector.</param>
    /// <summary>Applies the matrix to a vector, ignoring translation.</summary>
    /// <returns>Returns the mapped point.</returns>
    /// <remarks>
    ///     Mapping vectors ignores the translation component in the matrix. Use
    ///     <see cref="M:SkiaSharp.Matrix3x3.MapXY(System.Single,System.Single)" /> to take the translation into consideration.
    /// </remarks>
    public readonly unsafe SKPoint MapVector(float x, float y)
    {
        SKPoint skPoint;
        fixed (Matrix3X3* matrix = &this)
        {
            SkiaApi.sk_matrix_map_vector(matrix, x, y, &skPoint);
        }

        return skPoint;
    }

    /// <param name="result">
    ///     The array where the mapped results will be stored (needs to have the same number of elements of
    ///     the <paramref name="vectors" /> array).
    /// </param>
    /// <param name="vectors">The array of vectors to map.</param>
    /// <summary>Apply the to the array of vectors and return the mapped results..</summary>
    /// <remarks>
    ///     Mapping vectors ignores the translation component in the matrix. Use
    ///     <see cref="M:SkiaSharp.Matrix3x3.MapPoints(SkiaSharp.SKPoint[],SkiaSharp.SKPoint[])" /> to take the translation
    ///     into consideration.
    /// </remarks>
    public readonly unsafe void MapVectors(SKPoint[] result, SKPoint[] vectors)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (vectors == null)
        {
            throw new ArgumentNullException(nameof(vectors));
        }

        if (result.Length != vectors.Length)
        {
            throw new ArgumentException("Buffers must be the same size.");
        }

        fixed (Matrix3X3* matrix = &this)
        fixed (SKPoint* dst = result)
        fixed (SKPoint* src = vectors)
        {
            SkiaApi.sk_matrix_map_vectors(matrix, dst, src, result.Length);
        }
    }

    /// <param name="vectors">The array of vectors to map.</param>
    /// <summary>Applies the matrix to the array of vectors, ignoring translation, and returns the mapped results.</summary>
    /// <returns>Returns the new array allocated with the mapped results.</returns>
    /// <remarks>
    ///     Mapping vectors ignores the translation component in the matrix. Use
    ///     <see cref="M:SkiaSharp.Matrix3x3.MapPoints(SkiaSharp.SKPoint[])" /> to take the translation into consideration.
    /// </remarks>
    public readonly SKPoint[] MapVectors(SKPoint[] vectors)
    {
        var result = vectors != null ? new SKPoint[vectors.Length] : throw new ArgumentNullException(nameof(vectors));
        MapVectors(result, vectors);
        return result;
    }

    /// <param name="radius">The radius to map.</param>
    /// <summary>Calculates the mean radius of a circle after it has been mapped by this matrix.</summary>
    /// <returns>Returns the mean radius.</returns>
    /// <remarks />
    public readonly unsafe float MapRadius(float radius)
    {
        fixed (Matrix3X3* matrix = &this)
        {
            return SkiaApi.sk_matrix_map_radius(matrix, radius);
        }
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
