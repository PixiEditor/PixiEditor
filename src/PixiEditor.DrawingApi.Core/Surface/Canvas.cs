using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.Vector;

namespace PixiEditor.DrawingApi.Core.Surface
{
    public class Canvas
    {
        public void DrawPixel(VecI position, Paint drawingPaint) => DrawPixel(position.X, position.Y, drawingPaint);
        public void DrawPixel(int posX, int posY, Paint drawingPaint) => DrawingBackendApi.Current.CanvasOperations.DrawPixel(posX, posY, drawingPaint);

        public void DrawSurface(DrawingSurface original, int x, int y, Paint? paint) 
            => DrawingBackendApi.Current.CanvasOperations.DrawSurface(original, x, y, paint);
        
        public void DrawSurface(DrawingSurface original, int x, int y) => DrawSurface(original, x, y, null);
        
        public void DrawSurface(DrawingSurface surfaceToDraw, VecI size, Paint paint)
        {
            DrawSurface(surfaceToDraw, size.X, size.Y, paint);
        }

        public void DrawImage(Image image, int x, int y) => DrawingBackendApi.Current.CanvasOperations.DrawImage(image, x, y);
        
        public void DrawImage(Image image, RectD rect, Paint paint) => DrawingBackendApi.Current.CanvasOperations.DrawImage(image, rect, paint);

        public int Save()
        {
            return DrawingBackendApi.Current.CanvasOperations.Save();
        }

        public void Restore()
        {
            DrawingBackendApi.Current.CanvasOperations.Restore();
        }
        
        public void Scale(float s) => DrawingBackendApi.Current.CanvasOperations.Scale(s, s);

        /// <param name="sx">The amount to scale in the x-direction.</param>
        /// <param name="sy">The amount to scale in the y-direction.</param>
        /// <summary>Pre-concatenates the current matrix with the specified scale.</summary>
        public void Scale(float sx, float sy) => DrawingBackendApi.Current.CanvasOperations.Scale(sx, sy);

        /// <param name="size">The amount to scale.</param>
        /// <summary>Pre-concatenates the current matrix with the specified scale.</summary>
        public void Scale(Point size) => DrawingBackendApi.Current.CanvasOperations.Scale(size.X, size.Y);

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
            DrawingBackendApi.Current.CanvasOperations.Translate(translationX, translationY);
        }
        
        public void Translate(VecD vector) => Translate((float)vector.X, (float)vector.Y);

        public void DrawPath(VectorPath path, Paint paint)
        {
            DrawingBackendApi.Current.CanvasOperations.DrawPath(path, paint);
        }
        
        public void DrawPoint(VecI pos, Paint paint)
        {
            DrawingBackendApi.Current.CanvasOperations.DrawPoint(pos, paint);
        }

        public void DrawPoints(PointMode pointMode, Point[] points, Paint paint)
        {
            DrawingBackendApi.Current.CanvasOperations.DrawPoints(pointMode, points, paint);
        }

        public void DrawRect(int x, int y, int width, int height, Paint paint)
        {
            DrawingBackendApi.Current.CanvasOperations.DrawRect(x, y, width, height, paint);
        }
        
        public void DrawRect(RectI rect, Paint paint) => DrawRect(rect.X, rect.Y, rect.Width, rect.Height, paint);

        public void ClipPath(VectorPath clipPath) => ClipPath(clipPath, ClipOperation.Intersect);

        public void ClipPath(VectorPath clipPath, ClipOperation clipOperation) =>
            ClipPath(clipPath, clipOperation, false);
        
        public void ClipPath(VectorPath clipPath, ClipOperation clipOperation, bool antialias)
        {
            DrawingBackendApi.Current.CanvasOperations.ClipPath(clipPath, clipOperation, antialias);
        }

        public void ClipRect(RectD rect, ClipOperation clipOperation = ClipOperation.Intersect)
        {
            DrawingBackendApi.Current.CanvasOperations.ClipRect(rect, clipOperation);
        }

        public void Clear()
        {
            DrawingBackendApi.Current.CanvasOperations.Clear();
        }
        
        public void Clear(Color color)
        {
            DrawingBackendApi.Current.CanvasOperations.Clear(color);
        }

        public void DrawLine(VecI from, VecI to, Paint paint)
        {
            DrawingBackendApi.Current.CanvasOperations.DrawLine(from, to, paint);
        }

        public void Flush()
        {
            DrawingBackendApi.Current.CanvasOperations.Flush();
        }

        public void SetMatrix(Matrix3X3 finalMatrix)
        {
            DrawingBackendApi.Current.CanvasOperations.SetMatrix(finalMatrix);
        }

        public void RestoreToCount(int count)
        {
            DrawingBackendApi.Current.CanvasOperations.RestoreToCount(count);
        }

        public void DrawColor(Color color, BlendMode paintBlendMode)
        {
            DrawingBackendApi.Current.CanvasOperations.DrawColor(color, paintBlendMode);
        }

        public void RotateRadians(float dataAngle, float centerX, float centerY)
        {
            DrawingBackendApi.Current.CanvasOperations.RotateRadians(dataAngle, centerX, centerY);
        }

        public void DrawBitmap(Bitmap bitmap, int x, int y)
        {
            DrawingBackendApi.Current.CanvasOperations.DrawBitmap(bitmap, x, y);
        }
    }
}
