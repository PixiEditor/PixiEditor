using Avalonia.Skia;
using PixiEditor.DrawingApi.Core.Surface;
using SkiaSharp;

namespace PixiEditor.Engine.AvaloniaPlatform;

public class PixiEngineSkiaSurface : ISkiaSurface
{
    public DrawingSurface BackendSurface { get; }

    SKSurface ISkiaSurface.Surface => BackendSurface.Native as SKSurface;
    public bool CanBlit => false;
    public bool IsDisposed => BackendSurface.IsDisposed;
    public double RenderScaling { get; }
    

    public PixiEngineSkiaSurface(DrawingSurface surface, double renderScaling)
    {
        BackendSurface = surface;
        RenderScaling = renderScaling;
    }

    public void Blit(SKCanvas canvas)
    {
        throw new NotSupportedException();
    }

    public void Dispose()
    {
        BackendSurface.Dispose();
    }
}
