﻿using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.DrawingApi.Core.Surface.Vector;

namespace PixiEditor.DrawingApi.Core.Surface
{
    public class Canvas : NativeObject
    {
        public Canvas(IntPtr objPtr) : base(objPtr)
        {
        }
        
        public void DrawPixel(VecI position, Paint drawingPaint) => DrawPixel(position.X, position.Y, drawingPaint);
        public void DrawPixel(int posX, int posY, Paint drawingPaint) => 
            DrawingBackendApi.Current.CanvasImplementation.DrawPixel(ObjectPointer, posX, posY, drawingPaint);

        public void DrawSurface(DrawingSurface original, int x, int y, Paint? paint) 
            => DrawingBackendApi.Current.CanvasImplementation.DrawSurface(ObjectPointer, original, x, y, paint);
        
        public void DrawSurface(DrawingSurface original, int x, int y) => DrawSurface(original, x, y, null);
        
        public void DrawSurface(DrawingSurface surfaceToDraw, VecI size, Paint paint)
        {
            DrawSurface(surfaceToDraw, size.X, size.Y, paint);
        }

        public void DrawImage(Image image, int x, int y) => DrawingBackendApi.Current.CanvasImplementation.DrawImage(ObjectPointer, image, x, y);
        
        public void DrawImage(Image image, RectD rect, Paint paint) => 
            DrawingBackendApi.Current.CanvasImplementation.DrawImage(ObjectPointer, image, rect, paint);

        public int Save()
        {
            return DrawingBackendApi.Current.CanvasImplementation.Save(ObjectPointer);
        }

        public void Restore()
        {
            DrawingBackendApi.Current.CanvasImplementation.Restore(ObjectPointer);
        }
        
        public void Scale(float s) => DrawingBackendApi.Current.CanvasImplementation.Scale(ObjectPointer, s, s);

        /// <param name="sx">The amount to scale in the x-direction.</param>
        /// <param name="sy">The amount to scale in the y-direction.</param>
        /// <summary>Pre-concatenates the current matrix with the specified scale.</summary>
        public void Scale(float sx, float sy) => DrawingBackendApi.Current.CanvasImplementation.Scale(ObjectPointer, sx, sy);

        /// <param name="size">The amount to scale.</param>
        /// <summary>Pre-concatenates the current matrix with the specified scale.</summary>
        public void Scale(Point size) => DrawingBackendApi.Current.CanvasImplementation.Scale(ObjectPointer, size.X, size.Y);

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

        public void Translate(float translationX, float translationY)
        {
            DrawingBackendApi.Current.CanvasImplementation.Translate(ObjectPointer, translationX, translationY);
        }
        
        public void Translate(VecD vector) => Translate((float)vector.X, (float)vector.Y);

        public void DrawPath(VectorPath path, Paint paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawPath(ObjectPointer, path, paint);
        }
        
        public void DrawPoint(VecI pos, Paint paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawPoint(ObjectPointer, pos, paint);
        }

        public void DrawPoints(PointMode pointMode, Point[] points, Paint paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawPoints(ObjectPointer, pointMode, points, paint);
        }

        public void DrawRect(int x, int y, int width, int height, Paint paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawRect(ObjectPointer, x, y, width, height, paint);
        }
        
        public void DrawRect(RectI rect, Paint paint) => DrawRect(rect.X, rect.Y, rect.Width, rect.Height, paint);

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
        }
        
        public void Clear(Color color)
        {
            DrawingBackendApi.Current.CanvasImplementation.Clear(ObjectPointer, color);
        }

        public void DrawLine(VecI from, VecI to, Paint paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawLine(ObjectPointer, from, to, paint);
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
        }

        public void RotateRadians(float dataAngle, float centerX, float centerY)
        {
            DrawingBackendApi.Current.CanvasImplementation.RotateRadians(ObjectPointer, dataAngle, centerX, centerY);
        }

        public void DrawBitmap(Bitmap bitmap, int x, int y)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawBitmap(ObjectPointer, bitmap, x, y);
        }

        public override void Dispose()
        {
            DrawingBackendApi.Current.CanvasImplementation.Dispose(ObjectPointer);
        }
    }
}
