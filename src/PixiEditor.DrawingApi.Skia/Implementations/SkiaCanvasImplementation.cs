using System;
using PixiEditor.DrawingApi.Core.Bridge.Operations;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.DrawingApi.Core.Surface.Vector;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public sealed class SkiaCanvasImplementation : SkObjectImplementation<SKCanvas>, ICanvasImplementation
    {
        private readonly SkObjectImplementation<SKPaint> _paintImpl;
        private SkObjectImplementation<SKSurface> _surfaceImpl;
        private readonly SkObjectImplementation<SKImage> _imageImpl;
        private readonly SkObjectImplementation<SKBitmap> _bitmapImpl;
        private readonly SkObjectImplementation<SKPath> _pathImpl;

        public SkiaCanvasImplementation(SkObjectImplementation<SKPaint> paintImpl, SkObjectImplementation<SKImage> imageImpl, SkObjectImplementation<SKBitmap> bitmapImpl, SkObjectImplementation<SKPath> pathImpl)
        {
            _paintImpl = paintImpl;
            _imageImpl = imageImpl;
            _bitmapImpl = bitmapImpl;
            _pathImpl = pathImpl;
        }
        
        public void SetSurfaceImplementation(SkiaSurfaceImplementation surfaceImpl)
        {
            _surfaceImpl = surfaceImpl;
        }
        
        public void DrawPixel(IntPtr objectPointer, int posX, int posY, Paint drawingPaint)
        {
            ManagedInstances[objectPointer].DrawPoint(
                posX, 
                posY, 
                _paintImpl.ManagedInstances[drawingPaint.ObjectPointer]);
        }

        public void DrawSurface(IntPtr objPtr, DrawingSurface drawingSurface, int x, int y, Paint? paint)
        {
            ManagedInstances[objPtr]
                .DrawSurface(
                    _surfaceImpl.ManagedInstances[drawingSurface.ObjectPointer],
                    x, y, 
                    paint != null ? _paintImpl.ManagedInstances[paint.ObjectPointer] : null);
        }

        public void DrawImage(IntPtr objPtr, Image image, int x, int y)
        {
            ManagedInstances[objPtr]
                .DrawImage(
                    _imageImpl.ManagedInstances[image.ObjectPointer], x, y);
        }

        public int Save(IntPtr objPtr)
        {
            return ManagedInstances[objPtr].Save();
        }

        public void Restore(IntPtr objPtr)
        {
            ManagedInstances[objPtr].Restore();
        }

        public void Scale(IntPtr objPtr, float sizeX, float sizeY)
        {
            ManagedInstances[objPtr].Scale(sizeX, sizeY);
        }

        public void Translate(IntPtr objPtr, float translationX, float translationY)
        {
            ManagedInstances[objPtr].Translate(translationX, translationY);
        }

        public void DrawPath(IntPtr objPtr, VectorPath path, Paint paint)
        {
            ManagedInstances[objPtr].DrawPath(
                _pathImpl[path.ObjectPointer], 
                _paintImpl[paint.ObjectPointer]);
        }

        public void DrawPoint(IntPtr objPtr, VecI pos, Paint paint)
        {
            ManagedInstances[objPtr].DrawPoint(
                pos.X, 
                pos.Y, 
                _paintImpl[paint.ObjectPointer]);
        }

        public void DrawPoints(IntPtr objPtr, PointMode pointMode, Point[] points, Paint paint)
        {
            ManagedInstances[objPtr].DrawPoints(
                (SKPointMode)pointMode,
                CastUtility.UnsafeArrayCast<Point, SKPoint>(points),
                _paintImpl[paint.ObjectPointer]);
        }

        public void DrawRect(IntPtr objPtr, int x, int y, int width, int height, Paint paint)
        {
            ManagedInstances[objPtr].DrawRect(x, y, width, height, _paintImpl[paint.ObjectPointer]);
        }

        public void ClipPath(IntPtr objPtr, VectorPath clipPath, ClipOperation clipOperation, bool antialias)
        {
            SKCanvas canvas = ManagedInstances[objPtr];
            canvas.ClipPath(_pathImpl[clipPath.ObjectPointer], (SKClipOperation)clipOperation, antialias);
        }

        public void ClipRect(IntPtr objPtr, RectD rect, ClipOperation clipOperation)
        {
            SKCanvas canvas = ManagedInstances[objPtr];
            canvas.ClipRect(rect.ToSKRect(), (SKClipOperation)clipOperation);
        }

        public void Clear(IntPtr objPtr)
        {
            ManagedInstances[objPtr].Clear();
        }

        public void Clear(IntPtr objPtr, Color color)
        {
            ManagedInstances[objPtr].Clear(color.ToSKColor());
        }

        public void DrawLine(IntPtr objPtr, VecI from, VecI to, Paint paint)
        {
            ManagedInstances[objPtr].DrawLine(from.X, from.Y, to.X, to.Y, _paintImpl[paint.ObjectPointer]);
        }

        public void Flush(IntPtr objPtr)
        {
            ManagedInstances[objPtr].Flush();
        }

        public void SetMatrix(IntPtr objPtr, Matrix3X3 finalMatrix)
        {
            SKCanvas canvas = ManagedInstances[objPtr];
            canvas.SetMatrix(finalMatrix.ToSkMatrix());
        }

        public void RestoreToCount(IntPtr objPtr, int count)
        {
            ManagedInstances[objPtr].RestoreToCount(count);
        }

        public void DrawColor(IntPtr objPtr, Color color, BlendMode paintBlendMode)
        {
            ManagedInstances[objPtr].DrawColor(color.ToSKColor(), (SKBlendMode)paintBlendMode);
        }

        public void RotateRadians(IntPtr objPtr, float radians, float centerX, float centerY)
        {
            ManagedInstances[objPtr].RotateRadians(radians, centerX, centerY);
        }

        public void DrawImage(IntPtr objPtr, Image image, RectD rect, Paint paint)
        {
            ManagedInstances[objPtr].DrawImage(
                _imageImpl[image.ObjectPointer],
                rect.ToSKRect(), 
                _paintImpl[paint.ObjectPointer]);
        }

        public void DrawBitmap(IntPtr objPtr, Bitmap bitmap, int x, int y)
        {
            ManagedInstances[objPtr].DrawBitmap(_bitmapImpl[bitmap.ObjectPointer], x, y);
        }

        public void Dispose(IntPtr objectPointer)
        {
            ManagedInstances[objectPointer].Dispose();
            
            ManagedInstances.TryRemove(objectPointer, out _);
        }
    }
}
