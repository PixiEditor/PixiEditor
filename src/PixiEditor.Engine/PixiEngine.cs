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
    private IDrawingBackend _drawingBackend;
    
    internal PixiEngine(IDrawingBackend drawingBackend)
    {
        _drawingBackend = drawingBackend;
        ActiveEngine = this;
    }

    public virtual IWindow GetWindow(WindowOptions options)
    {
        return Silk.NET.Windowing.Window.Create(options);   
    }

    protected void SetBackendActive()
    {
        DrawingBackendApi.SetupBackend(_drawingBackend);
    }
}
