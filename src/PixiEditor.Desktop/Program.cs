using System;
using Avalonia;
using Avalonia.Logging;
using Drawie.Interop.Avalonia;
using Drawie.Interop.VulkanAvalonia;
using PixiEditor.Helpers;

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
    {
        bool openGlPreferred = string.Equals(RenderApiPreferenceManager.TryReadRenderApiPreference(), "opengl", StringComparison.OrdinalIgnoreCase);
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions()
            {
                RenderingMode = openGlPreferred ? [ Win32RenderingMode.Wgl, Win32RenderingMode.Vulkan] : [ Win32RenderingMode.Vulkan, Win32RenderingMode.Wgl],
                OverlayPopups = true,
            })
            .With(new X11PlatformOptions()
            {
                RenderingMode = openGlPreferred ? [ X11RenderingMode.Glx, X11RenderingMode.Vulkan] : [ X11RenderingMode.Vulkan, X11RenderingMode.Glx],
                OverlayPopups = true,
            })
            .With(new SkiaOptions()
            {
                MaxGpuResourceSizeBytes = 1024 * 600 * 4 * 12 * 4 // quadruple the default size
            })
            .WithDrawie()
#if DEBUG
            .LogToTrace(LogEventLevel.Verbose, "Vulkan")
#endif
            .LogToTrace();
    }
}
