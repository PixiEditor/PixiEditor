using Avalonia;
using System;
using PixiEditor.Engine;
using PixiEditor.Engine.AvaloniaPlatform;
using PixiEditor.Numerics;
using SkiaSharp;

namespace PixiEngineAvaloniaApp;

class Program
{
    public static void Main(string[] args)
    {
        Window mainWindow = new Window("PixiEngine Avalonia App", new VecI(600, 600));
        AvaloniaHost host = null;
        mainWindow.Init += () =>
        {
            BuildAvaloniaApp();
            host = new AvaloniaHost(mainWindow.Size, new SampleView());
        };
        
        mainWindow.Render += (surface, dt) => host.Render(surface, dt);
        mainWindow.Update += dt => host.Update(dt);
        mainWindow.Resize += size => host.Resize(size);
        
        SkiaPixiEngine.CreateAndRun(mainWindow);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePixiEngine()
            .SetupWithoutStarting();
}
