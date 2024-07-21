using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Skia;
using SkiaSharp;

namespace PixiEditor.Engine;

public abstract class PixiEngine
{
    private IDrawingBackend _drawingBackend;
    
    internal PixiEngine(IDrawingBackend drawingBackend)
    {
        _drawingBackend = drawingBackend;
    }

    protected virtual void Setup()
    {
        DrawingBackendApi.SetupBackend(_drawingBackend);
    }
}
