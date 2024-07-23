using Avalonia;
using Avalonia.Skia;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using Silk.NET.Core.Contexts;
using Silk.NET.OpenGL;
using SkiaSharp;

namespace PixiEditor.Engine.AvaloniaPlatform;

public class PixiEngineSkiaGpu : ISkiaGpu
{
    private GRContext context;
    private IGLContext glContext;

    public bool IsLost => context.IsAbandoned;

    public PixiEngineSkiaGpu(SkiaPixiEngine engine)
    {
        context = engine.GrContext;
        glContext = engine.GlContext;
    }
    

    public object? TryGetFeature(Type featureType)
    {
        return null;
    }

    public IDisposable EnsureCurrent()
    {
        return EmptyDisposable.Instance;
    }

    public ISkiaGpuRenderTarget? TryCreateRenderTarget(IEnumerable<object> surfaces) =>
        surfaces.OfType<PixiEngineSkiaSurface>().FirstOrDefault() is { } surface 
            ? new PixiEngineSkiaGpuRenderTarget(surface, context)
            : null;

    ISkiaSurface? ISkiaGpu.TryCreateSurface(PixelSize size, ISkiaGpuRenderSession? session)
    {
        if(size.Width <= 0 || size.Height <= 0)
            return null;
        
        return new PixiEngineSkiaSurface(CreateSurface(size.Width, size.Height), session?.ScaleFactor ?? 1);
    }

    public DrawingSurface CreateSurface(int sizeX, int sizeY)
    {
        return DrawingSurface.Create(new ImageInfo(sizeX, sizeY));
    }
    
    public void Dispose()
    {
        context.Dispose();
    }
}
