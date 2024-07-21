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
    public static Compositor Compositor => _compositor ?? throw new PlatformNotInitializedException($"({nameof(PixiEnginePlatform)})was not initialized.");
    public static void Initialize()
    {
        SkiaPixiEngine engine = SkiaPixiEngine.Create();
        var graphics = new PixiEngineGraphics(engine);
        
        AvaloniaLocator.CurrentMutable
            .Bind<IPlatformGraphics>().ToConstant(graphics)
            .Bind<IRenderTimer>().ToConstant(new DefaultRenderTimer(60))
            .Bind<IWindowingPlatform>().ToConstant(new PixiEngineWindowingPlatform());

        _compositor = new Compositor(graphics);
    }
}
