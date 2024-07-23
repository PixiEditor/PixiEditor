using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.OpenGL;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Skia;
using PixiEditor.Numerics;
using SkiaSharp;

namespace InjectedDrawingApiAvalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var grContext = App.GetGrContext();
            DrawingBackendApi.SetupBackend(new SkiaDrawingBackend(CreateGpuSurface));
            
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private SKSurface CreateGpuSurface(VecI size)
    {
        WriteableBitmap bitmap = new WriteableBitmap(new PixelSize(size.X, size.Y), new Vector(96, 96), PixelFormat.Bgra8888);
        using var fb = bitmap.Lock();
        SKImageInfo info = new(size.X, size.Y, SKColorType.Bgra8888, SKAlphaType.Premul);
        return SKSurface.Create(info, fb.Address, fb.RowBytes);
    }

    public static GRContext GetGrContext()
    {
        Compositor compositor = Compositor.TryGetDefaultCompositor();
        var interop = compositor.TryGetCompositionGpuInterop();
        var contextSharingFeature =
            compositor.TryGetRenderInterfaceFeature(typeof(IOpenGlTextureSharingRenderInterfaceContextFeature)).Result
                as IOpenGlTextureSharingRenderInterfaceContextFeature;

        if (contextSharingFeature.CanCreateSharedContext)
        {
            IGlContext? glContext = contextSharingFeature.CreateSharedContext();
            glContext.MakeCurrent();
            return GRContext.CreateGl(GRGlInterface.Create(glContext.GlInterface.GetProcAddress));
        }

        return null;
        /*var contextFactory = AvaloniaLocator.Current.GetRequiredService<IPlatformGraphicsOpenGlContextFactory>();
        var ctx = contextFactory.CreateContext(null);
        ctx.MakeCurrent();
        var ctxInterface = GRGlInterface.Create(ctx.GlInterface.GetProcAddress);
        var grContext = GRContext.CreateGl(ctxInterface);
        return grContext;*/
    }
}
