namespace PixiEditor.Numerics;

/// <summary>
///     Represents an ordered pair of floating-point x- and y-coordinates that defines a point in a two-dimensional
///     plane.
/// </summary>
public struct Point : IEquatable<Point>
{
    /// <summary>Represents a new instance of the <see cref="Point" /> class with member data left uninitialized.</summary>
    public static readonly Point Empty;

    public Point(float x, float y)
    {
        X = x;
        Y = y;
    }

    public Point(VecI pointPos)
    {
        X = pointPos.X;
        Y = pointPos.Y;
    }
    

    /// <summary>Gets a value indicating whether this point is empty.</summary>
    /// <value />
    public readonly bool IsEmpty => this == Empty;

    /// <summary>Gets the Euclidean distance from the origin (0, 0).</summary>
    /// <value />
    /// <remarks />
    public readonly float Length => (float)Math.Sqrt((X * (double)X) + (Y * (double)Y));

    /// <summary>Gets the Euclidean distance squared from the origin (0, 0).</summary>
    /// <value />
    /// <remarks />
    public readonly float LengthSquared => (float)((X * (double)X) + (Y * (double)Y));

    /// <param name="p">The offset value.</param>
    /// <summary>Translates a given point by a specified offset.</summary>
    /// <remarks />
    public void Offset(Point p)
    {
        X += p.X;
        Y += p.Y;
    }

    /// <param name="dx">The offset in the x-direction.</param>
    /// <param name="dy">The offset in the y-direction.</param>
    /// <summary>Translates a given point by a specified offset.</summary>
    public void Offset(float dx, float dy)
    {
        X += dx;
        Y += dy;
    }

    /// <summary>Converts this <see cref="Point" /> to a human readable string.</summary>
    /// <returns>A string that represents this <see cref="Point" />.</returns>
    public override readonly string ToString()
    {
        return string.Format("{{X={0}, Y={1}}}", X, Y);
    }

    /// <param name="point">The point to normalize.</param>
    /// <summary>Returns a point with the same direction as the specified point, but with a length of one.</summary>
    /// <returns>Returns a point with a length of one.</returns>
    public static Point Normalize(Point point)
    {
        var num = 1.0 / Math.Sqrt((point.X * (double)point.X) + (point.Y * (double)point.Y));
        return new Point(point.X * (float)num, point.Y * (float)num);
    }

    /// <param name="point">The first point.</param>
    /// <param name="other">The second point.</param>
    /// <summary>Calculate the Euclidean distance between two points.</summary>
    /// <returns>Returns the Euclidean distance between two points.</returns>
    public static float Distance(Point point, Point other)
    {
        var num1 = point.X - other.X;
        var num2 = point.Y - other.Y;
        return (float)Math.Sqrt((num1 * (double)num1) + (num2 * (double)num2));
    }

    /// <param name="point">The first point.</param>
    /// <param name="other">The second point.</param>
    /// <summary>Calculate the Euclidean distance squared between two points.</summary>
    /// <returns>Returns the Euclidean distance squared between two points.</returns>
    public static float DistanceSquared(Point point, Point other)
    {
        var num1 = point.X - other.X;
        var num2 = point.Y - other.Y;
        return (float)((num1 * (double)num1) + (num2 * (double)num2));
    }

    /// <param name="point">The point to reflect.</param>
    /// <param name="normal">The normal.</param>
    /// <summary>Returns the reflection of a point off a surface that has the specified normal.</summary>
    /// <returns>Returns the reflection of a point.</returns>
    public static Point Reflect(Point point, Point normal)
    {
        var num = (float)((point.X * (double)point.X) + (point.Y * (double)point.Y));
        return new Point(point.X - (2f * num * normal.X), point.Y - (2f * num * normal.Y));
    }

    /// <param name="pt">The point to translate</param>
    /// <param name="sz">The offset size.</param>
    /// <summary>Translates a given point by a specified size.</summary>
    /// <returns>Returns the translated point.</returns>
    public static Point Add(Point pt, VecI sz)
    {
        return pt + sz;
    }

    /// <param name="pt">The point to translate.</param>
    /// <param name="sz">The offset size.</param>
    /// <summary>Translates a given point by a specified size.</summary>
    /// <returns>Returns the translated point.</returns>
    public static Point Add(Point pt, VecD sz)
    {
        return pt + sz;
    }

    /// <param name="pt">The point to translate.</param>
    /// <param name="sz">The offset value.</param>
    /// <summary>Translates a given point by a specified offset.</summary>
    /// <returns>Returns the translated point.</returns>
    public static Point Add(Point pt, Point sz)
    {
        return pt + sz;
    }

    /// <param name="pt">The <see cref="Point" /> to translate.</param>
    /// <param name="sz">
    ///     The <see cref="VecD" /> that specifies the numbers to subtract from the coordinates of
    ///     <paramref name="pt" />.
    /// </param>
    /// <summary>Translates a <see cref="Point" /> by the negative of a specified size.</summary>
    /// <returns>The translated <see cref="Point" />.</returns>
    public static Point Subtract(Point pt, VecI sz)
    {
        return pt - sz;
    }

    /// <param name="pt">The <see cref="Point" /> to translate.</param>
    /// <param name="sz">
    ///     The <see cref="VecD" /> that specifies the numbers to subtract from the coordinates of
    ///     <paramref name="pt" />.
    /// </param>
    /// <summary>Translates a <see cref="Point" /> by the negative of a specified size.</summary>
    /// <returns>The translated <see cref="Point" />.</returns>
    public static Point Subtract(Point pt, VecD sz)
    {
        return pt - sz;
    }

    /// <param name="pt">The <see cref="Point" /> to translate.</param>
    /// <param name="sz">The offset that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
    /// <summary>Translates a given point by the negative of a specified offset.</summary>
    /// <returns>The translated <see cref="Point" />.</returns>
    public static Point Subtract(Point pt, Point sz)
    {
        return pt - sz;
    }

    /// <param name="pt">The point to translate.</param>
    /// <param name="sz">The offset size.</param>
    /// <summary>Translates a given point by a specified size.</summary>
    /// <returns>Returns the translated point.</returns>
    public static Point operator +(Point pt, VecI sz)
    {
        return new Point(pt.X + sz.X, pt.Y + sz.Y);
    }

    /// <param name="pt">The point to translate.</param>
    /// <param name="sz">The offset size.</param>
    /// <summary>Translates a given point by a specified size.</summary>
    /// <returns>Returns the translated point.</returns>
    public static Point operator +(Point pt, VecD sz)
    {
        return new Point(pt.X + (float)sz.X, pt.Y + (float)sz.Y);
    }

    /// <param name="pt">The point to translate.</param>
    /// <param name="sz">The offset value.</param>
    /// <summary>Translates a given point by a specified offset.</summary>
    /// <returns>Returns the translated point.</returns>
    public static Point operator +(Point pt, Point sz)
    {
        return new Point(pt.X + sz.X, pt.Y + sz.Y);
    }

    /// <param name="pt">The <see cref="Point" /> to translate.</param>
    /// <param name="sz">
    ///     The <see cref="VecI" /> that specifies the numbers to subtract from the coordinates of
    ///     <paramref name="pt" />.
    /// </param>
    /// <summary>Translates a <see cref="Point" /> by the negative of a given <see cref="VecI" />.</summary>
    /// <returns>The translated <see cref="Point" />.</returns>
    public static Point operator -(Point pt, VecI sz)
    {
        return new Point(pt.X - sz.X, pt.Y - sz.Y);
    }

    /// <param name="pt">The <see cref="Point" /> to translate.</param>
    /// <param name="sz">
    ///     The <see cref="VecD" /> that specifies the numbers to subtract from the coordinates of
    ///     <paramref name="pt" />.
    /// </param>
    /// <summary>Translates a <see cref="Point" /> by the negative of a given <see cref="VecD" />.</summary>
    /// <returns>The translated <see cref="Point" />.</returns>
    public static Point operator -(Point pt, VecD sz)
    {
        return new Point(pt.X - (float)sz.X, pt.Y - (float)sz.Y);
    }

    /// <param name="pt">The <see cref="Point" /> to translate.</param>
    /// <param name="sz">The point that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
    /// <summary>Translates a given point by the negative of a specified offset.</summary>
    /// <returns>The translated <see cref="Point" />.</returns>
    public static Point operator -(Point pt, Point sz)
    {
        return new Point(pt.X - sz.X, pt.Y - sz.Y);
    }

    /// <summary>Gets or sets the x-coordinate of this <see cref="Point" />.</summary>
    /// <value />
    public float X { get; set; }

    /// <summary>Gets or sets the x-coordinate of this <see cref="Point" />.</summary>
    /// <value />
    public float Y { get; set; }

    /// <param name="obj">The <see cref="Point" /> to test.</param>
    /// <summary>
    ///     Specifies whether this <see cref="Point" /> contains the same coordinates as the specified
    ///     <see cref="Point" />.
    /// </summary>
    /// <returns>
    ///     This method returns true if <paramref name="obj" /> has the same coordinates as this
    ///     <see cref="Point" />.
    /// </returns>
    public readonly bool Equals(Point obj)
    {
        return X == (double)obj.X && Y == (double)obj.Y;
    }

    /// <param name="obj">The <see cref="T:System.Object" /> to test.</param>
    /// <summary>
    ///     Specifies whether this <see cref="Point" /> contains the same coordinates as the specified
    ///     <see cref="T:System.Object" />.
    /// </summary>
    /// <returns>
    ///     This method returns true if <paramref name="obj" /> is a <see cref="Point" /> and has the same
    ///     coordinates as this <see cref="Point" />.
    /// </returns>
    /// <remarks />
    public override readonly bool Equals(object obj)
    {
        return obj is Point Point && Equals(Point);
    }

    /// <param name="left">A <see cref="Point" /> to compare.</param>
    /// <param name="right">A <see cref="Point" /> to compare with.</param>
    /// <summary>
    ///     Compares two <see cref="Point" /> structures. The result specifies whether the values of the
    ///     <see cref="P:.Point.X" /> and <see cref="P:.Point.Y" /> properties of the two
    ///     <see cref="Point" /> structures are equal.
    /// </summary>
    /// <returns>
    ///     true if the <see cref="P:.Point.X" /> and <see cref="P:.Point.Y" /> values of the left and
    ///     right <see cref="Point" /> structures are equal; otherwise, false.
    /// </returns>
    public static bool operator ==(Point left, Point right)
    {
        return left.Equals(right);
    }

    /// <param name="left">A <see cref="Point" /> to compare.</param>
    /// <param name="right">A <see cref="Point" /> to compare with.</param>
    /// <summary>Determines whether the coordinates of the specified points are not equal.</summary>
    /// <returns>
    ///     true if the <see cref="Point.X" /> and <see cref="Point.Y" /> values of the left and
    ///     right <see cref="Point" /> structures differ; otherwise, false.
    /// </returns>
    public static bool operator !=(Point left, Point right)
    {
        return !left.Equals(right);
    }
    
    public static explicit operator Point(VecI vec)
    {
        return new Point(vec.X, vec.Y);
    }
    
    public static explicit operator Point(VecD vec)
    {
        return new Point((float)vec.X, (float)vec.Y);
    }
    
    public static explicit operator VecI(Point vec)
    {
        return new VecI((int)vec.X, (int)vec.Y);
    }
    
    public static explicit operator VecD(Point vec)
    {
        return new VecD(vec.X, vec.Y);
    }

    /// <summary>Calculates the hashcode for this point.</summary>
    /// <returns>Returns the hashcode for this point.</returns>
    /// <remarks>
    ///     You should avoid depending on GetHashCode for unique values, as two <see cref="T:System.Drawing.Point" />
    ///     objects with the same values for their X and Y properties may return the same hash code. This behavior could change
    ///     in a future release.
    /// </remarks>
    public override readonly int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(X);
        hashCode.Add(Y);
        return hashCode.ToHashCode();
    }
}
