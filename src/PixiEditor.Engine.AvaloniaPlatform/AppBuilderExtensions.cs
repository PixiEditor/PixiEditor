using Avalonia;

namespace PixiEditor.Engine.AvaloniaPlatform;

public static class AppBuilderExtensions
{
    public static AppBuilder UsePixiEngine(this AppBuilder builder)
        => builder
            .UseStandardRuntimePlatformSubsystem()
            .UseSkia()
            .UseWindowingSubsystem(PixiEnginePlatform.Initialize);
}
