using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Skia;
using Silk.NET.Core.Contexts;
using Silk.NET.Windowing;
using SkiaSharp;

namespace PixiEditor.Engine;

public abstract class PixiEngine
{
    public static PixiEngine ActiveEngine { get; protected set; }
    
    public GRContext GrContext { get; protected set; }
    public IGLContext GlContext { get; protected set; }
    public ulong Ticks { get; private set; }

    private IDrawingBackend _drawingBackend;
    
    internal PixiEngine(IDrawingBackend drawingBackend)
    {
        _drawingBackend = drawingBackend;
        ActiveEngine = this;
    }

    protected void SetBackendActive()
    {
        DrawingBackendApi.SetupBackend(_drawingBackend);
    }
    
    protected void Tick(double deltaTime)
    {
        Ticks += (ulong)deltaTime;
    }
}
