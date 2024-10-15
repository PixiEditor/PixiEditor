using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.DrawingApi.Core.Surfaces.Vector;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Surfaces
{
    public delegate void SurfaceChangedEventHandler(RectD? changedRect);

    public class Canvas : NativeObject
    {
        public override object Native => DrawingBackendApi.Current.CanvasImplementation.GetNativeCanvas(ObjectPointer);
        public Matrix3X3 TotalMatrix => DrawingBackendApi.Current.CanvasImplementation.GetTotalMatrix(ObjectPointer);

        public event SurfaceChangedEventHandler? Changed;

        public Canvas(IntPtr objPtr) : base(objPtr)
        {
        }

        public void DrawPixel(VecI position, Paint drawingPaint) => DrawPixel(position.X, position.Y, drawingPaint);

        public void DrawPixel(int posX, int posY, Paint drawingPaint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawPixel(ObjectPointer, posX, posY, drawingPaint);
            Changed?.Invoke(new RectD(posX, posY, 1, 1));
        }

        public void DrawSurface(DrawingSurface original, float x, float y, Paint? paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawSurface(ObjectPointer, original, x, y, paint);
            Changed?.Invoke(null);
        }

        public void DrawSurface(DrawingSurface original, float x, float y) => DrawSurface(original, x, y, null);

        public void DrawSurface(DrawingSurface surfaceToDraw, VecI position, Paint paint)
        {
            DrawSurface(surfaceToDraw, position.X, position.Y, paint);
        }

        public void DrawImage(Image image, float x, float y) =>
            DrawingBackendApi.Current.CanvasImplementation.DrawImage(ObjectPointer, image, x, y);
        
        public void DrawImage(Image image, float x, float y, Paint paint) =>
            DrawingBackendApi.Current.CanvasImplementation.DrawImage(ObjectPointer, image, x, y, paint);

        public void DrawImage(Image image, RectD destRect, Paint paint) =>
            DrawingBackendApi.Current.CanvasImplementation.DrawImage(ObjectPointer, image, destRect, paint);
        
        public void DrawImage(Image image, RectD sourceRect, RectD destRect, Paint paint) =>
            DrawingBackendApi.Current.CanvasImplementation.DrawImage(ObjectPointer, image, sourceRect, destRect, paint);

        public int Save()
        {
            return DrawingBackendApi.Current.CanvasImplementation.Save(ObjectPointer);
        }

        public void Restore()
        {
            DrawingBackendApi.Current.CanvasImplementation.Restore(ObjectPointer);
        }

        public void Scale(float s) => Scale(s, s);

        /// <param name="size">The amount to scale.</param>
        /// <summary>Pre-concatenates the current matrix with the specified scale.</summary>
        public void Scale(Point size) => Scale(size.X, size.Y);

        /// <param name="sx">The amount to scale in the x-direction.</param>
        /// <param name="sy">The amount to scale in the y-direction.</param>
        /// <summary>Pre-concatenates the current matrix with the specified scale.</summary>
        public void Scale(float sx, float sy)
        {
            DrawingBackendApi.Current.CanvasImplementation.Scale(ObjectPointer, sx, sy);
        }

        /// <param name="sx">The amount to scale in the x-direction.</param>
        /// <param name="sy">The amount to scale in the y-direction.</param>
        /// <param name="px">The x-coordinate for the scaling center.</param>
        /// <param name="py">The y-coordinate for the scaling center.</param>
        /// <summary>Pre-concatenates the current matrix with the specified scale, at the specific offset.</summary>
        public void Scale(float sx, float sy, float px, float py)
        {
            Translate(px, py);
            Scale(sx, sy);
            Translate(-px, -py);
        }

        public void Translate(VecD vector) => Translate((float)vector.X, (float)vector.Y);

        public void Translate(float translationX, float translationY)
        {
            DrawingBackendApi.Current.CanvasImplementation.Translate(ObjectPointer, translationX, translationY);
        }

        public void DrawPath(VectorPath path, Paint paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawPath(ObjectPointer, path, paint);
            Changed?.Invoke(path.Bounds);
        }

        public void DrawPoint(VecD pos, Paint paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawPoint(ObjectPointer, pos, paint);
            Changed?.Invoke(new RectD(pos.X, pos.Y, 1, 1));
        }

        public void DrawPoints(PointMode pointMode, Point[] points, Paint paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawPoints(ObjectPointer, pointMode, points, paint);
            Changed?.Invoke(RectD.FromPoints(points));
        }

        public void DrawRect(float x, float y, float width, float height, Paint paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawRect(ObjectPointer, x, y, width, height, paint);
            Changed?.Invoke(new RectD(x, y, width, height));
        }

        public void DrawCircle(float centerX, float centerY, float radius, Paint paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawCircle(ObjectPointer, centerX, centerY, radius, paint);
            Changed?.Invoke(new RectD(centerX - radius, centerY - radius, radius * 2, radius * 2));
        }

        public void DrawCircle(VecD center, float radius, Paint paint) =>
            DrawCircle((float)center.X, (float)center.Y, radius, paint);

        public void DrawOval(float centerX, float centerY, float radiusX, float radiusY, Paint paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawOval(ObjectPointer, centerX, centerY, radiusX, radiusY, paint);
            Changed?.Invoke(new RectD(centerX - radiusX, centerY - radiusY, radiusX * 2, radiusY * 2));
        }

        public void DrawOval(VecD center, VecD radius, Paint paint) =>
            DrawOval((float)center.X, (float)center.Y, (float)radius.X, (float)radius.Y, paint);

        public void DrawRect(RectD rect, Paint paint) => DrawRect((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, paint);

        public void ClipPath(VectorPath clipPath) => ClipPath(clipPath, ClipOperation.Intersect);

        public void ClipPath(VectorPath clipPath, ClipOperation clipOperation) =>
            ClipPath(clipPath, clipOperation, false);

        public void ClipPath(VectorPath clipPath, ClipOperation clipOperation, bool antialias)
        {
            DrawingBackendApi.Current.CanvasImplementation.ClipPath(ObjectPointer, clipPath, clipOperation, antialias);
        }

        public void ClipRect(RectD rect, ClipOperation clipOperation = ClipOperation.Intersect)
        {
            DrawingBackendApi.Current.CanvasImplementation.ClipRect(ObjectPointer, rect, clipOperation);
        }

        public void Clear()
        {
            DrawingBackendApi.Current.CanvasImplementation.Clear(ObjectPointer);
            Changed?.Invoke(null);
        }

        public void Clear(Color color)
        {
            DrawingBackendApi.Current.CanvasImplementation.Clear(ObjectPointer, color);
            Changed?.Invoke(null);
        }

        public void DrawLine(VecD from, VecD to, Paint paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawLine(ObjectPointer, from, to, paint);
            Changed?.Invoke(new RectD(from, to));
        }

        public void Flush()
        {
            DrawingBackendApi.Current.CanvasImplementation.Flush(ObjectPointer);
        }

        public void SetMatrix(Matrix3X3 finalMatrix)
        {
            DrawingBackendApi.Current.CanvasImplementation.SetMatrix(ObjectPointer, finalMatrix);
        }

        public void RestoreToCount(int count)
        {
            DrawingBackendApi.Current.CanvasImplementation.RestoreToCount(ObjectPointer, count);
        }

        public void DrawColor(Color color, BlendMode paintBlendMode)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawColor(ObjectPointer, color, paintBlendMode);
            Changed?.Invoke(null);
        }

        public void RotateRadians(float radians, float centerX, float centerY)
        {
            DrawingBackendApi.Current.CanvasImplementation.RotateRadians(ObjectPointer, radians, centerX, centerY);
        }

        public void RotateDegrees(float degrees, float centerX, float centerY)
        {
            DrawingBackendApi.Current.CanvasImplementation.RotateDegrees(ObjectPointer, degrees, centerX, centerY);
        }

        public void DrawBitmap(Bitmap bitmap, int x, int y)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawBitmap(ObjectPointer, bitmap, x, y);
            Changed?.Invoke(null);
        }

        public void DrawPaint(Paint paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawPaint(ObjectPointer, paint);
            Changed?.Invoke(null);
        }
        
        public int SaveLayer()
        {
            return DrawingBackendApi.Current.CanvasImplementation.SaveLayer(ObjectPointer);
        }

        public int SaveLayer(Paint paint)
        {
            return DrawingBackendApi.Current.CanvasImplementation.SaveLayer(ObjectPointer, paint);
        }

        public override void Dispose()
        {
            DrawingBackendApi.Current.CanvasImplementation.Dispose(ObjectPointer);
        }
    }
}
