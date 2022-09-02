using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.Vector;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Core.Surface
{
    public class Canvas
    {
        public void DrawPixel(int posX, int posY, Paint drawingPaint) => DrawingBackendApi.Current.CanvasOperations.DrawPixel(posX, posY, drawingPaint);

        public void DrawSurface(DrawingSurface original, int x, int y) 
            => DrawingBackendApi.Current.CanvasOperations.DrawSurface(original, x, y);

        public void DrawImage(Image image, int x, int y) => DrawingBackendApi.Current.CanvasOperations.DrawImage(image, x, y);

        public int Save()
        {
            return DrawingBackendApi.Current.CanvasOperations.Save();
        }

        public void Restore()
        {
            DrawingBackendApi.Current.CanvasOperations.Restore();
        }

        public void Scale(float multiplier)
        {
            DrawingBackendApi.Current.CanvasOperations.Scale(multiplier);
        }

        public void Translate(VecI vector)
        {
            DrawingBackendApi.Current.CanvasOperations.Translate(vector);
        }

        public void DrawPath(VectorPath path, Paint paint)
        {
            DrawingBackendApi.Current.CanvasOperations.DrawPath(path, paint);
        }
    }
}
