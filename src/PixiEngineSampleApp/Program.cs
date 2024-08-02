using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.DrawingApi.Skia;
using PixiEditor.Engine;
using PixiEditor.Numerics;
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
        SkiaPixiEngine.Create().SetupBackendWindowLess();
        
        Window mainWindow = new Window("PixiEngineSampleApp", new VecI(1200, 600));
        mainWindow.Render += WindowOnRender;
        mainWindow.Init += Init;
        
        mainWindow.Run();
    }

    private static void Init()
    {
        _drawingSurface = DrawingSurface.Create(new ImageInfo(100, 100));
    }

    private static void WindowOnRender(SKSurface surface, double deltaTime)
    {
        surface.Canvas.DrawSurface((SKSurface)_drawingSurface.Native, 0, 0, _screenPaint);

        _lastTime += deltaTime;
        if (_lastTime > 1)
        {
            _drawingSurface.Canvas.Clear();
            _drawingSurface.Canvas.DrawText("Seconds: " + _time, 10, 10, _paint);
            _drawingSurface.Flush();
            _time++;
            _lastTime = 0;
        }
    }
}
