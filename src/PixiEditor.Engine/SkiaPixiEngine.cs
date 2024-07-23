using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Skia;
using PixiEditor.DrawingApi.Skia.Exceptions;
using PixiEditor.Numerics;
using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SkiaSharp;

namespace PixiEditor.Engine;

public sealed class SkiaPixiEngine : PixiEngine
{
    private SkiaDrawingBackend _drawingBackend;
    
    private IWindow _mainWindow;
    
    private SkiaPixiEngine(IDrawingBackend drawingBackend) : base(drawingBackend)
    {
        _drawingBackend = (SkiaDrawingBackend)drawingBackend;
    }
    
    public static SkiaPixiEngine Create()
    {
        return new SkiaPixiEngine(new SkiaDrawingBackend());
    }

    public void SetupBackendWindowLess()
    {
        _mainWindow = Silk.NET.Windowing.Window.Create(WindowOptions.Default with { IsVisible = false, Size = new Vector2D<int>(1, 1)}); 
        _mainWindow.Load += () =>
        {
            GRGlGetProcedureAddressDelegate proc = Glfw.GetApi().GetProcAddress;
            GrContext = GRContext.CreateGl(GRGlInterface.Create(proc));
            GlContext = _mainWindow.GLContext;
            Setup(GrContext);
        };
        
        _mainWindow.Update += Tick;

        _mainWindow.Initialize();
    }

    public void Setup(GRContext context)
    {
        SetBackendActive();
        _drawingBackend.GraphicsContext = context;
        GrContext = context;
        _drawingBackend.Setup();
    }
    
    public void RunWithWindow(Window window)
    {
        window.InitWithGrContext += Setup;
        window.Update += Tick;
        window.Run();
    }
    
    public static SkiaPixiEngine CreateAndRun(Window window)
    {
        SkiaPixiEngine engine = Create();
        engine.RunWithWindow(window);
        return engine;
    }
}
