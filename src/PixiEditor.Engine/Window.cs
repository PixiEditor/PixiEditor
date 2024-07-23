using PixiEditor.DrawingApi.Core.Surface;
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
    protected IWindow _window;
    private GRContext _grContext;

    private SKSurface? frontBufferSurface;

    internal IWindow Native => _window;

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

    public bool TopMost
    {
        get => _window.TopMost;
        set => _window.TopMost = value;
    }

    public SKSurface FramebufferSurface => frontBufferSurface;
    public event Action<double> Update;

    public event Action<SKSurface, double> Render;
    public event Action Init;
    public event Action<VecI> Resize;
    internal event Action<GRContext> InitWithGrContext;

    private bool ran = false;

    public Window(string title = "", VecI size = default)
    {
        WindowOptions options = WindowOptions.Default with
        {
            Title = title, Size = size == default ? new Vector2D<int>(600, 600) : size.ToVector2D()
        };

        _window = Silk.NET.Windowing.Window.Create(options);
        
        if (!_window.IsInitialized)
        {
            _window.Load += () =>
            {
                gl = GL.GetApi(_window);
                
                _grContext = PixiEngine.ActiveEngine.GrContext;

                if (_grContext == null)
                {
                    GRGlGetProcedureAddressDelegate proc = Glfw.GetApi().GetProcAddress;
                    _grContext = GRContext.CreateGl(GRGlInterface.Create(proc));

                    InitWithGrContext?.Invoke(_grContext);
                }

                frontBufferSurface = SKSurface.Create(new SKImageInfo(1200, 600));
                InitSkiaSurface(_window.FramebufferSize);
                Init?.Invoke();
            };
        }
        
        _window.Update += (deltaTime) =>
        {
            Update?.Invoke(deltaTime);
        };

        _window.FramebufferResize += (newSize) =>
        {
            frontBufferSurface?.Dispose();
            InitSkiaSurface(newSize);
        };

        _window.Render += OnRender;
        _window.Resize += (newSize) => Resize?.Invoke(newSize.ToVecI());
    }

    public void Run()
    {
        if (_window.IsInitialized)
        {
            if (frontBufferSurface == null)
            {
                InitSkiaSurface(_window.FramebufferSize);
            }

            Init?.Invoke();
        }

        ran = true;
        _window.Run();
    }

    internal void Initialize()
    {
        _window.Initialize();
    }

    private void OnRender(double deltaTime)
    {
        frontBufferSurface.Canvas.Clear(SKColors.Transparent);
        Render?.Invoke(frontBufferSurface, deltaTime);
        frontBufferSurface.Flush(true);
    }

    private void InitSkiaSurface(Vector2D<int> size)
    { ;
        var frameBuffer = new GRGlFramebufferInfo(0, SKColorType.Rgba8888.ToGlSizedFormat());
        GRBackendRenderTarget target = new GRBackendRenderTarget(size.X, size.Y, 4, 0, frameBuffer);
        frontBufferSurface = SKSurface.Create(_grContext, target, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
    }

    public void Show()
    {
        if (!ran)
        {
            Run();
        }
        
        _window.IsVisible = true;
    }

    public void Hide()
    {
        _window.IsVisible = false;
    }

    public void Activate()
    {
    }

    public void Close()
    {
        _window.Close();
    }
}
