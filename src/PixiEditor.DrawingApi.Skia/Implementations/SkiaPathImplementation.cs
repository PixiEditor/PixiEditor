using System;
using PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.Vector;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public class SkiaPathImplementation : SkObjectImplementation<SKPath>, IVectorPathImplementation
    {
        public PathFillType GetFillType(VectorPath path)
        {
            return (PathFillType)ManagedInstances[path.ObjectPointer].FillType;
        }

        public void SetFillType(VectorPath path, PathFillType fillType)
        {
            ManagedInstances[path.ObjectPointer].FillType = (SKPathFillType)fillType;
        }

        public PathConvexity GetConvexity(VectorPath path)
        {
            return (PathConvexity)ManagedInstances[path.ObjectPointer].Convexity;
        }

        public void SetConvexity(VectorPath path, PathConvexity convexity)
        {
            ManagedInstances[path.ObjectPointer].Convexity = (SKPathConvexity)convexity;
        }

        public void Dispose(VectorPath path)
        {
            ManagedInstances[path.ObjectPointer].Dispose();
        }

        public bool IsPathOval(VectorPath path)
        {
            return ManagedInstances[path.ObjectPointer].IsOval;
        }

        public bool IsRoundRect(VectorPath path)
        {
            return ManagedInstances[path.ObjectPointer].IsRoundRect;
        }

        public bool IsLine(VectorPath path)
        {
            return ManagedInstances[path.ObjectPointer].IsLine;
        }

        public bool IsRect(VectorPath path)
        {
            return ManagedInstances[path.ObjectPointer].IsRect;
        }

        public PathSegmentMask GetSegmentMasks(VectorPath path)
        {
            return (PathSegmentMask)ManagedInstances[path.ObjectPointer].SegmentMasks;
        }

        public int GetVerbCount(VectorPath path)
        {
            return ManagedInstances[path.ObjectPointer].VerbCount;
        }

        public int GetPointCount(VectorPath path)
        {
            return ManagedInstances[path.ObjectPointer].PointCount;
        }

        public IntPtr Create()
        {
            SKPath path = new SKPath();
            ManagedInstances[path.Handle] = path;
            return path.Handle;
        }

        public IntPtr Clone(VectorPath other)
        {
            SKPath path = new SKPath(ManagedInstances[other.ObjectPointer]);
            ManagedInstances[path.Handle] = path;
            return path.Handle;
        }

        public RectD GetTightBounds(VectorPath vectorPath)
        {
            SKRect rect = ManagedInstances[vectorPath.ObjectPointer].TightBounds;
            return new RectD(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        public void Transform(VectorPath vectorPath, Matrix3X3 matrix)
        {
            ManagedInstances[vectorPath.ObjectPointer].Transform(matrix.ToSkMatrix());
        }

        public RectD GetBounds(VectorPath vectorPath)
        {
            SKRect rect = ManagedInstances[vectorPath.ObjectPointer].Bounds;
            return new RectD(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        public void Reset(VectorPath vectorPath)
        {
            ManagedInstances[vectorPath.ObjectPointer].Reset();
        }

        public void MoveTo(VectorPath vectorPath, Point point)
        {
            ManagedInstances[vectorPath.ObjectPointer].MoveTo(point.ToSkPoint());
        }

        public void LineTo(VectorPath vectorPath, Point point)
        {
            ManagedInstances[vectorPath.ObjectPointer].LineTo(point.ToSkPoint());
        }

        public void QuadTo(VectorPath vectorPath, Point mid, Point point)
        {
            ManagedInstances[vectorPath.ObjectPointer].QuadTo(mid.ToSkPoint(), point.ToSkPoint());
        }

        public void CubicTo(VectorPath vectorPath, Point mid1, Point mid2, Point point)
        {
            ManagedInstances[vectorPath.ObjectPointer].CubicTo(mid1.ToSkPoint(), mid2.ToSkPoint(), point.ToSkPoint());
        }

        public void ArcTo(VectorPath vectorPath, RectI oval, int startAngle, int sweepAngle, bool forceMoveTo)
        {
            ManagedInstances[vectorPath.ObjectPointer].ArcTo(oval.ToSkRect(), startAngle, sweepAngle, forceMoveTo);
        }

        public void AddOval(VectorPath vectorPath, RectI borders)
        {
            ManagedInstances[vectorPath.ObjectPointer].AddOval(borders.ToSkRect());
        }

        public VectorPath Op(VectorPath vectorPath, VectorPath ellipsePath, VectorPathOp pathOp)
        {
            SKPath skPath = ManagedInstances[vectorPath.ObjectPointer].Op(ManagedInstances[ellipsePath.ObjectPointer], (SKPathOp)pathOp);
            ManagedInstances[skPath.Handle] = skPath;
            return new VectorPath(skPath.Handle);
        }

        public void Close(VectorPath vectorPath)
        {
            ManagedInstances[vectorPath.ObjectPointer].Close();
        }
    }
}
