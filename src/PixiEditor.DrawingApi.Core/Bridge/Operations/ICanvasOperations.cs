using PixiEditor.DrawingApi.Core.Surface;

namespace PixiEditor.DrawingApi.Core.Bridge.Operations
{
    public interface ICanvasOperations
    {
        public void DrawPixel(int posX, int posY, Paint drawingPaint);
        public void DrawSurface(DrawingSurface drawingSurface, int x, int y);
        public void DrawImage(Image image, int x, int y);
    }
}
