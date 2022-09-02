using System;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.Vector;

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
}
