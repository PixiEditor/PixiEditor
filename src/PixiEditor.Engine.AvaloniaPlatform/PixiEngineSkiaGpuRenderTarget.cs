using Avalonia.Skia;
using SkiaSharp;

namespace PixiEditor.Engine.AvaloniaPlatform;

public class PixiEngineSkiaGpuRenderTarget : ISkiaGpuRenderTarget
{
    private readonly PixiEngineSkiaSurface _surface;
    private readonly GRContext _context;
    private readonly double _renderScaling;
    
    public bool IsCorrupted => _surface.IsDisposed || _context.IsAbandoned || _renderScaling != _surface.RenderScaling; 
    
    public PixiEngineSkiaGpuRenderTarget(PixiEngineSkiaSurface surface, GRContext context)
    {
        _surface = surface;
        _context = context;
        _renderScaling = surface.RenderScaling;
    }

    public ISkiaGpuRenderSession BeginRenderingSession()
    {
        return new PixiEngineGpuRenderSession(_surface, _context);
    }

    public void Dispose() { }
}
