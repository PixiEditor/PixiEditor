using Avalonia.Skia;
using SkiaSharp;

namespace PixiEditor.Engine.AvaloniaPlatform;

public class PixiEngineGpuRenderSession : ISkiaGpuRenderSession
{
    public PixiEngineSkiaSurface Surface { get; }
    public GRContext GrContext { get; }
    
    SKSurface ISkiaGpuRenderSession.SkSurface => Surface.BackendSurface.Native as SKSurface;
    double ISkiaGpuRenderSession.ScaleFactor => Surface.RenderScaling;
    GRSurfaceOrigin ISkiaGpuRenderSession.SurfaceOrigin => GRSurfaceOrigin.BottomLeft;
    
    public PixiEngineGpuRenderSession(PixiEngineSkiaSurface surface, GRContext context)
    {
        Surface = surface;
        GrContext = context;
    }
    
    public void Dispose()
    {
        (Surface.BackendSurface.Native as SKSurface)?.Flush(true);
    }
}
