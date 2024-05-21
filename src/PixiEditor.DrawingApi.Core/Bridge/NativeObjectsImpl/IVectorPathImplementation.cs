using System;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.Vector;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;

public interface IVectorPathImplementation
{
    public PathFillType GetFillType(VectorPath path);
    public void SetFillType(VectorPath path, PathFillType fillType);
    public PathConvexity GetConvexity(VectorPath path);
    public void SetConvexity(VectorPath path, PathConvexity convexity);
    public void Dispose(VectorPath path);
    public bool IsPathOval(VectorPath path);
    public bool IsRoundRect(VectorPath path);
    public bool IsLine(VectorPath path);
    public bool IsRect(VectorPath path);
    public PathSegmentMask GetSegmentMasks(VectorPath path);
    public int GetVerbCount(VectorPath path);
    public int GetPointCount(VectorPath path);
    public IntPtr Create();
    public IntPtr Clone(VectorPath other);
    public RectD GetTightBounds(VectorPath vectorPath);
    public void Transform(VectorPath vectorPath, Matrix3X3 matrix);
    public RectD GetBounds(VectorPath vectorPath);
    public void Reset(VectorPath vectorPath);
    public void AddRect(VectorPath path, RectI rect, PathDirection direction);
    public void MoveTo(VectorPath vectorPath, Point point);
    public void LineTo(VectorPath vectorPath, Point point);
    public void QuadTo(VectorPath vectorPath, Point mid, Point point);
    public void CubicTo(VectorPath vectorPath, Point mid1, Point mid2, Point point);
    public void ArcTo(VectorPath vectorPath, RectI oval, int startAngle, int sweepAngle, bool forceMoveTo);
    public void AddOval(VectorPath vectorPath, RectI borders);
    public VectorPath Op(VectorPath vectorPath, VectorPath ellipsePath, VectorPathOp pathOp);
    public void Close(VectorPath vectorPath);
    public VectorPath Simplify(VectorPath vectorPath);
    public string ToSvgPathData(VectorPath vectorPath);
    public bool Contains(VectorPath vectorPath, float x, float y);
    public void AddPath(VectorPath vectorPath, VectorPath path, AddPathMode mode);
    public object GetNativePath(IntPtr objectPointer);
}
