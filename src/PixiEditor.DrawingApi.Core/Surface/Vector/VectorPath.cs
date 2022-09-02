using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.DrawingApi.Core.Surface.Vector;

/// <summary>An interface for native compound geometric path implementations.</summary>
/// <remarks>A path encapsulates compound (multiple contour) geometric paths consisting of straight line segments, quadratic curves, and cubic curves.</remarks>
public class VectorPath : NativeObject
{
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

    public VectorPath() : base(DrawingBackendApi.Current.PathImplementation.Create())
    {
    }

    public VectorPath(VectorPath other) : base(DrawingBackendApi.Current.PathImplementation.Clone(other))
    {
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.PathImplementation.Dispose(this);
    }
}
