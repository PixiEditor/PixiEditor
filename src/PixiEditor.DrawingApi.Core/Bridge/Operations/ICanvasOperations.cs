using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.Vector;

namespace PixiEditor.DrawingApi.Core.Bridge.Operations
{
    public interface ICanvasOperations
    {
        public void DrawPixel(int posX, int posY, Paint drawingPaint);
        public void DrawSurface(DrawingSurface drawingSurface, int x, int y, Paint? paint);
        public void DrawImage(Image image, int x, int y);
        public int Save();
        public void Restore();
        public void Scale(float sizeX, float sizeY);
        public void Translate(float translationX, float translationY);
        public void DrawPath(VectorPath path, Paint paint);
        public void DrawPoint(VecI pos, Paint paint);
        public void DrawPoints(PointMode pointMode, Point[] points, Paint paint);
        public void DrawRect(int x, int y, int width, int height, Paint paint);
        public void ClipPath(VectorPath clipPath, ClipOperation clipOperation, bool antialias);
        public void ClipRect(RectD rect, ClipOperation clipOperation);
        public void Clear();
        public void Clear(Color color);
        public void DrawLine(VecI from, VecI to, Paint paint);
        public void Flush();
        public void SetMatrix(Matrix3X3 finalMatrix);
        public void RestoreToCount(int count);
        public void DrawColor(Color color, BlendMode paintBlendMode);
        public void RotateRadians(float dataAngle, float centerX, float centerY);
        public void DrawImage(Image image, RectD rect, Paint paint);
        public void DrawBitmap(Bitmap bitmap, int x, int y);
    }
}
