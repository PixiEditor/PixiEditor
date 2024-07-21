using Avalonia.Platform;
using PixiEditor.Engine.AvaloniaPlatform.Windowing;

namespace PixiEditor.Engine.AvaloniaPlatform;

public class PixiEngineWindowingPlatform() : IWindowingPlatform
{
    public IWindowImpl CreateWindow()
    {
        PixiEngineTopLevel topLevel = new PixiEngineTopLevel(new PixiEngineTopLevelImpl(PixiEnginePlatform.Compositor));
        Window window = new Window();
        return new PixiEngineWindowImpl(window, topLevel);
    }

    public IWindowImpl CreateEmbeddableWindow()
    {
        return CreateWindow();
    }

    public ITrayIconImpl? CreateTrayIcon()
    {
        return null;
    }
}
