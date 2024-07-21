using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
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
        Window mainWindow = new Window("PixiEngineSampleApp", new VecI(1200, 600));
        mainWindow.Render += WindowOnRender;
        mainWindow.Init += Init;
        
        SkiaPixiEngine.CreateAndRun(mainWindow);
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
            _time++;
            _lastTime = 0;
        }
    }
}
