using Avalonia.Platform;

namespace PixiEditor.Engine.AvaloniaPlatform;

public class PixiEngineGraphics : IPlatformGraphics, IDisposable
{
    private PixiEngineSkiaGpu? _gpu;
    private SkiaPixiEngine _engine;
    
    public PixiEngineGraphics(SkiaPixiEngine engine)
    {
        _engine = engine;
    }

    public IPlatformGraphicsContext CreateContext()
    {
        throw new System.NotSupportedException();
    } 
    
    public PixiEngineSkiaGpu GetSharedContext()
    {
        if (_gpu == null)
        {
            _gpu = new PixiEngineSkiaGpu(_engine);
        }
        
        return _gpu;
    }

    IPlatformGraphicsContext IPlatformGraphics.GetSharedContext() => GetSharedContext();

    public bool UsesSharedContext => true;
    public void Dispose()
    {
        _gpu?.Dispose();
    }
}
