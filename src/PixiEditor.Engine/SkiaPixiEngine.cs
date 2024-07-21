using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Skia;
using PixiEditor.DrawingApi.Skia.Exceptions;
using SkiaSharp;

namespace PixiEditor.Engine;

public sealed class SkiaPixiEngine : PixiEngine
{
    private SkiaDrawingBackend _drawingBackend;
    private SkiaPixiEngine(IDrawingBackend drawingBackend) : base(drawingBackend)
    {
        _drawingBackend = (SkiaDrawingBackend)drawingBackend;
    }

    public static SkiaPixiEngine Create()
    {
        return new SkiaPixiEngine(new SkiaDrawingBackend());
    }

    public void Setup(GRContext context)
    {
        Setup();
        _drawingBackend.GraphicsContext = context;
        _drawingBackend.Setup();
    }
    
    public void RunWithWindow(Window window)
    {
        window.InitWithGrContext += Setup;
        window.Run();
    }
    
    public static SkiaPixiEngine CreateAndRun(Window window)
    {
        SkiaPixiEngine engine = Create();
        engine.RunWithWindow(window);
        return engine;
    }
}
