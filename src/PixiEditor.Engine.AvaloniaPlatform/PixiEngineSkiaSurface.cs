using Avalonia.Skia;
using PixiEditor.DrawingApi.Core.Surface;
using SkiaSharp;

namespace PixiEditor.Engine.AvaloniaPlatform;

public class PixiEngineSkiaSurface : ISkiaSurface
{
    public DrawingSurface BackendSurface { get; }

    public SKSurface Surface => nonDrawingSurface ?? BackendSurface?.Native as SKSurface;
    public bool CanBlit => false;
    public bool IsDisposed
    {
        get
        {
            if (nonDrawingSurface != null)
            {
                return false;
            }
            
            return BackendSurface.IsDisposed;
        }
    }

    public double RenderScaling { get; }

    private SKSurface? nonDrawingSurface;

    public PixiEngineSkiaSurface(DrawingSurface surface, double renderScaling)
    {
        BackendSurface = surface;
        RenderScaling = renderScaling;
    }

    public PixiEngineSkiaSurface(SKSurface surface, double scaling)
    {
       nonDrawingSurface = surface;
       RenderScaling = scaling;
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
