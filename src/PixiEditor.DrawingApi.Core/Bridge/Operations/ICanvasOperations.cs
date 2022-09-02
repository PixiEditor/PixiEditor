using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.Vector;

namespace PixiEditor.DrawingApi.Core.Bridge.Operations
{
    public interface ICanvasOperations
    {
        public void DrawPixel(int posX, int posY, Paint drawingPaint);
        public void DrawSurface(DrawingSurface drawingSurface, int x, int y);
        public void DrawImage(Image image, int x, int y);
        public int Save();
        public void Restore();
        public void Scale(float multiplier);
        public void Translate(VecI vector);
        public void DrawPath(VectorPath path, Paint paint);
    }
}
