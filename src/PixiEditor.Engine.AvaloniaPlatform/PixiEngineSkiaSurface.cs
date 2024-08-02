using Avalonia.Skia;
using PixiEditor.DrawingApi.Core.Surfaces;
using SkiaSharp;

namespace PixiEditor.Engine.AvaloniaPlatform;

public class PixiEngineSkiaSurface : ISkiaSurface
{
    public DrawingSurface BackendSurface { get; }

    public SKSurface Surface => BackendSurface?.Native as SKSurface;
    public bool CanBlit => false;
    public bool IsDisposed
    {
        get
        {
            return BackendSurface.IsDisposed;
        }
    }

    public double RenderScaling { get; set; }

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
