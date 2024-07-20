using DrawingApi.MinimalGpuSetup;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.DrawingApi.Skia;
using Silk.NET.GLFW;
using SkiaSharp;

public static class Program
{
    private static DrawingSurface _drawingSurface;
    private static Paint _paint = new Paint();
    private static SKPaint _screenPaint = new SKPaint();
    
    private static int _time = 0;
    private static double _lastTime = 0;
    
    public static void Main()
    {
       OpenGlSkiaWindow window = new OpenGlSkiaWindow();
       window.Render += WindowOnRender;
       window.Init += Init;
       window.Run();
    }
    
    private static void Init(GRContext context)
    {
        DrawingBackendApi.SetupBackend(new SkiaDrawingBackend(context));
        _drawingSurface = DrawingSurface.Create(new ImageInfo(100, 100), true);
    }

    private static void WindowOnRender(SKSurface surface, double deltaTime)
    {
        surface.Canvas.DrawSurface((SKSurface)_drawingSurface.Native, 0, 0, _screenPaint);
        
        _lastTime += deltaTime;
        if (_lastTime > 1)
        {
            _drawingSurface.Canvas.Clear();
            _drawingSurface.Canvas.DrawText("Seconds: " + _time, 10, 10, _paint);
            _time++;
            _lastTime = 0;
        }
    }
}
