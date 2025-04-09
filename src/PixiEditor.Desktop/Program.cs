using System;
using Avalonia;
using Avalonia.Logging;
using Drawie.Interop.VulkanAvalonia;

namespace PixiEditor.Desktop;

public class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions()
            {
                RenderingMode = new Win32RenderingMode[] { Win32RenderingMode.Vulkan },
                OverlayPopups = true
            })
            .With(new X11PlatformOptions()
            {
                RenderingMode = new X11RenderingMode[] { X11RenderingMode.Vulkan },
                OverlayPopups = true,
            })
            .WithDrawie()
            .LogToTrace(LogEventLevel.Verbose, "Vulkan")
            .LogToTrace();
}
