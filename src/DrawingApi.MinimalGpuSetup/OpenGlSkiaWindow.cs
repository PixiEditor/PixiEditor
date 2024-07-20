using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SkiaSharp;

namespace DrawingApi.MinimalGpuSetup;

public class OpenGlSkiaWindow
{
    private GL gl;
    private IWindow _window;
    private GRContext _grContext;

    private SKSurface frontBufferSurface;
    
    public event Action<SKSurface, double> Render;
    public event Action<GRContext> Init;

    public OpenGlSkiaWindow()
    {
        WindowOptions options = WindowOptions.Default with
        {
            Title = "OpenGL Window", Size = new Vector2D<int>(1200, 600)
        };

        _window = Window.Create(options);

        _window.Load += () =>
        {
            gl = GL.GetApi(_window);
            frontBufferSurface = SKSurface.Create(new SKImageInfo(1200, 600));
            
            _window.GLContext.MakeCurrent();

            InitSkiaSurface();
            
            Init?.Invoke(_grContext);
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
