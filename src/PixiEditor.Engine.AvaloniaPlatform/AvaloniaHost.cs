using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using PixiEditor.Engine.AvaloniaPlatform.Exceptions;
using PixiEditor.Numerics;
using SkiaSharp;

namespace PixiEditor.Engine.AvaloniaPlatform;

public class AvaloniaHost
{
    public VecI Size { get; }
    
    private PixiEngineTopLevel _topLevel;
    public AvaloniaHost(VecI size, Control control)
    {
        Size = size;
        
        var locator = AvaloniaLocator.Current;
        
        if(locator.GetService<IPlatformGraphics>() is not PixiEngineGraphics graphics)
        {
            throw new PlatformNotInitializedException($"No PixiEngine grapics fround. Did you call UsePixiEngine()?.");
        }
        
        PixiEngineTopLevel topLevel = new PixiEngineTopLevel(new PixiEngineTopLevelImpl(PixiEnginePlatform.Compositor, graphics))
        {
            Background = null
        };
        topLevel.Content = control;
        
        topLevel.Impl.SetRenderSize(new PixelSize(size.X, size.Y), 1);
        
        topLevel.Prepare();
        topLevel.StartRendering();
        
        _topLevel = topLevel;
    }

    public void Update(double dt)
    {
        PixiEnginePlatform.TriggerRenderTick();

        _topLevel.Impl.OnDraw(new Rect(0, 0, Size.X, Size.Y));
    }

    public void Render(SKSurface targetSurface, double dt)
    {
        var surface = _topLevel.Impl.Surface;
        targetSurface.Canvas.DrawSurface(surface.Surface, new SKPoint(0, 0));
    }
    
    public void Resize(VecI newSize)
    {
        _topLevel.Impl.SetRenderSize(new PixelSize(newSize.X, newSize.Y), 1);
    }
}
