using Avalonia.Platform;
using PixiEditor.Engine.AvaloniaPlatform.Windowing;

namespace PixiEditor.Engine.AvaloniaPlatform;

public class PixiEngineWindowingPlatform() : IWindowingPlatform
{
    public IWindowImpl CreateWindow()
    {
        Window window = new Window();
        return new PixiEngineWindowImpl(window);
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
