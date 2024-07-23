using Avalonia;
using Avalonia.Controls.Platform;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using PixiEditor.Engine.AvaloniaPlatform.Exceptions;

namespace PixiEditor.Engine.AvaloniaPlatform;

internal class PixiEnginePlatform
{
    private static Compositor? _compositor;
    private static ManualRenderTimer? _renderTimer;
    public static Compositor Compositor => _compositor ?? throw new PlatformNotInitializedException($"({nameof(PixiEnginePlatform)})was not initialized.");
    public static void Initialize()
    {
        var graphics = new PixiEngineGraphics((SkiaPixiEngine)PixiEngine.ActiveEngine);
        
        AvaloniaLocator.CurrentMutable
            .Bind<IPlatformGraphics>().ToConstant(graphics)
            .Bind<IRenderTimer>().ToConstant(new ManualRenderTimer())
            .Bind<IWindowingPlatform>().ToConstant(new PixiEngineWindowingPlatform(graphics));

        _compositor = new Compositor(graphics);
        _renderTimer = new ManualRenderTimer();
    }
    
    public static void TriggerRenderTick()
    {
        if (_renderTimer is null)
        {
            return;
        }
        
        _renderTimer.TriggerTick(new TimeSpan((long) (PixiEngine.ActiveEngine.Ticks * 10UL)));
    }
}
