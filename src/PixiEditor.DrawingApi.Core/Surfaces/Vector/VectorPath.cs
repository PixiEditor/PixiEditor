using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Surfaces.Vector;

/// <summary>An interface for native compound geometric path implementations.</summary>
/// <remarks>A path encapsulates compound (multiple contour) geometric paths consisting of straight line segments, quadratic curves, and cubic curves.</remarks>
public class VectorPath : NativeObject
{
    public override object Native => DrawingBackendApi.Current.PathImplementation.GetNativePath(ObjectPointer);

    public PathFillType FillType
    {
        get => DrawingBackendApi.Current.PathImplementation.GetFillType(this);
        set => DrawingBackendApi.Current.PathImplementation.SetFillType(this, value);
    }

    public PathConvexity Convexity 
    {
        get => DrawingBackendApi.Current.PathImplementation.GetConvexity(this);
        set => DrawingBackendApi.Current.PathImplementation.SetConvexity(this, value);
    }

    /// <summary>Gets a value indicating whether the path is a single oval or circle.</summary>
    public bool IsOval => DrawingBackendApi.Current.PathImplementation.IsPathOval(this);

    /// <summary>Gets a value indicating whether the path is a single, round rectangle.</summary>
    public bool IsRoundRect => DrawingBackendApi.Current.PathImplementation.IsRoundRect(this);

    /// <summary>Gets a value indicating whether the path is a single, straight line.</summary>
    public bool IsLine => DrawingBackendApi.Current.PathImplementation.IsLine(this);

    /// <summary>Gets a value indicating whether the path is a single rectangle.</summary>
    public bool IsRect => DrawingBackendApi.Current.PathImplementation.IsRect(this);

    /// <summary>Gets a set of flags indicating if the path contains one or more segments of that type.</summary>
    public PathSegmentMask SegmentMasks => DrawingBackendApi.Current.PathImplementation.GetSegmentMasks(this);

    /// <summary>Gets the number of verbs in the path.</summary>
    public int VerbCount => DrawingBackendApi.Current.PathImplementation.GetVerbCount(this);

    /// <summary>Gets the number of points on the path.</summary>
    public int PointCount => DrawingBackendApi.Current.PathImplementation.GetPointCount(this);
    
    /// <summary>Gets the "tight" bounds of the path. the control points of curves are excluded.</summary>
    /// <value>The tight bounds of the path.</value>
    public RectD TightBounds => DrawingBackendApi.Current.PathImplementation.GetTightBounds(this);

    public bool IsEmpty => VerbCount == 0;
    public RectD Bounds => DrawingBackendApi.Current.PathImplementation.GetBounds(this);
    
    public bool IsDisposed { get; private set; }
    
    public VectorPath(IntPtr nativePointer) : base(nativePointer)
    {
    }

    public VectorPath() : base(DrawingBackendApi.Current.PathImplementation.Create())
    {
    }

    public VectorPath(VectorPath other) : base(DrawingBackendApi.Current.PathImplementation.Clone(other))
    {
    }
    
    /// <param name="matrix">The matrix to use for transformation.</param>
    /// <summary>Applies a transformation matrix to the all the elements in the path.</summary>
    public void Transform(Matrix3X3 matrix) => DrawingBackendApi.Current.PathImplementation.Transform(this, matrix);

    public override void Dispose()
    {
        DrawingBackendApi.Current.PathImplementation.Dispose(this);
        IsDisposed = true;
    }

    public void Reset()
    {
        DrawingBackendApi.Current.PathImplementation.Reset(this);
    }

    public void MoveTo(Point point)
    {
        DrawingBackendApi.Current.PathImplementation.MoveTo(this, point);
    }

    public void LineTo(Point point)
    {
        DrawingBackendApi.Current.PathImplementation.LineTo(this, point);
    }

    public void QuadTo(Point mid, Point point)
    {
        DrawingBackendApi.Current.PathImplementation.QuadTo(this, mid, point);
    }

    public void CubicTo(Point mid1, Point mid2, Point point)
    {
        DrawingBackendApi.Current.PathImplementation.CubicTo(this, mid1, mid2, point);
    }

    public void ArcTo(RectI oval, int startAngle, int sweepAngle, bool forceMoveTo)
    {
        DrawingBackendApi.Current.PathImplementation.ArcTo(this, oval, startAngle, sweepAngle, forceMoveTo);
    }

    public void AddOval(RectI borders)
    {
        DrawingBackendApi.Current.PathImplementation.AddOval(this, borders);
    }

    /// <summary>
    ///     Compute the result of a logical operation on two paths.
    /// </summary>
    /// <param name="other">Other path.</param>
    /// <param name="pathOp">Logical operand.</param>
    /// <returns>Returns the resulting path if the operation was successful, otherwise null.</returns>
    public VectorPath Op(VectorPath other, VectorPathOp pathOp)
    {
        return DrawingBackendApi.Current.PathImplementation.Op(this, other, pathOp);
    }

    /// <summary>
    ///     Closes current contour.
    /// </summary>
    public void Close()
    {
        DrawingBackendApi.Current.PathImplementation.Close(this);
    }

    public string ToSvgPathData()
    {
        return DrawingBackendApi.Current.PathImplementation.ToSvgPathData(this);
    }

    public void AddRect(RectI rect, PathDirection direction = PathDirection.Clockwise)
    {
        DrawingBackendApi.Current.PathImplementation.AddRect(this, rect, direction);
    }

    public void AddPath(VectorPath path, AddPathMode mode)
    {
        DrawingBackendApi.Current.PathImplementation.AddPath(this, path, mode);
    }

    public bool Contains(float x, float y)
    {
        return DrawingBackendApi.Current.PathImplementation.Contains(this, x, y);
    }

    public VectorPath Simplify()
    {
        return DrawingBackendApi.Current.PathImplementation.Simplify(this);
    }
}

public enum PathDirection
{
    Clockwise,
    CounterClockwise
}

public enum AddPathMode
{
    Append,
    Extend
}
