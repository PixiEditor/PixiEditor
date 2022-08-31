using PixiEditor.DrawingApi.Core.Bridge;

namespace PixiEditor.DrawingApi.Core.Surface
{
    public class Canvas
    {
        public void DrawPixel(int posX, int posY, Paint drawingPaint) => DrawingBackendApi.Current.CanvasOperations.DrawPixel(posX, posY, drawingPaint);

        public void DrawSurface(DrawingSurface original, int x, int y) 
            => DrawingBackendApi.Current.CanvasOperations.DrawSurface(original, x, y);

        public void DrawImage(Image image, int x, int y) => DrawingBackendApi.Current.CanvasOperations.DrawImage(image, x, y);
    }
}
