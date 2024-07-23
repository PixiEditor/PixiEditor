using Avalonia.Skia;
using SkiaSharp;

namespace PixiEditor.Engine.AvaloniaPlatform;

public class PixiEngineGpuRenderSession : ISkiaGpuRenderSession
{
    public PixiEngineSkiaSurface Surface { get; }
    public GRContext GrContext { get; }

    SKSurface ISkiaGpuRenderSession.SkSurface => Surface.Surface;
    double ISkiaGpuRenderSession.ScaleFactor => Surface.RenderScaling;
    GRSurfaceOrigin ISkiaGpuRenderSession.SurfaceOrigin => GRSurfaceOrigin.BottomLeft;
    
    public PixiEngineGpuRenderSession(PixiEngineSkiaSurface surface, GRContext context)
    {
        Surface = surface;
        GrContext = context;
    }
    
    public void Dispose()
    {
        Surface.Surface?.Flush(true);
    }
}
