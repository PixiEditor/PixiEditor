using PixiEditor.Engine.Helpers;
using PixiEditor.Numerics;
using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SkiaSharp;

namespace PixiEditor.Engine;

public class Window
{
    private GL gl;
    private IWindow _window;
    private GRContext _grContext;

    private SKSurface frontBufferSurface;
    
    public string Title
    {
        get => _window.Title;
        set => _window.Title = value;
    }
    
    public VecI Size
    {
        get => _window.Size.ToVecI();
        set => _window.Size = value.ToVector2D();
    }

    public event Action<SKSurface, double> Render;
    public event Action Init;
    internal event Action<GRContext> InitWithGrContext;

    public Window(string title, VecI size)
    {
        WindowOptions options = WindowOptions.Default with
        {
            Title = title, Size = size.ToVector2D()
        };

        _window = Silk.NET.Windowing.Window.Create(options);

        _window.Load += () =>
        {
            gl = GL.GetApi(_window);
            frontBufferSurface = SKSurface.Create(new SKImageInfo(1200, 600));

            _window.GLContext.MakeCurrent();

            InitSkiaSurface();

            InitWithGrContext?.Invoke(_grContext);
            Init?.Invoke();
        };

        _window.Render += OnRender;
    }

    public void Run()
    {
        _window.Run();
    }

    private void OnRender(double deltaTime)
    {
        frontBufferSurface.Canvas.Clear(SKColors.White);
        Render?.Invoke(frontBufferSurface, deltaTime);
        frontBufferSurface.Canvas.Flush();
    }

    private void InitSkiaSurface()
    {
        _grContext = GRContext.CreateGl(GRGlInterface.Create(Glfw.GetApi().GetProcAddress));
        var frameBuffer = new GRGlFramebufferInfo(0, SKColorType.RgbaF16.ToGlSizedFormat());
        GRBackendRenderTarget target = new GRBackendRenderTarget(_window.Size.X, _window.Size.Y, 4, 0, frameBuffer);
        frontBufferSurface = SKSurface.Create(_grContext, target, GRSurfaceOrigin.BottomLeft, SKColorType.RgbaF16);
    }
}
