using System;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Vulkan;
using Drawie.Interop.VulkanAvalonia;
using PixiEditor.Helpers;

namespace PixiEditor.Desktop;

public class Program
{
    private static IClassicDesktopStyleApplicationLifetime? currentLifetime;
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, lifetime =>
        {
            currentLifetime = lifetime;
        });
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        bool openGlPreferred = false;
        try
        {
            openGlPreferred = string.Equals(RenderApiPreferenceManager.TryReadRenderApiPreference(), "opengl",
                StringComparison.OrdinalIgnoreCase);

            if (!openGlPreferred)
            {
                var cmdArgs = Environment.GetCommandLineArgs();
                if (cmdArgs is { Length: > 0 })
                {
                    openGlPreferred = cmdArgs.Any(arg =>
                        string.Equals(arg, "--opengl", StringComparison.OrdinalIgnoreCase));
                }
            }
        }
        catch (Exception ex)
        {
        }

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions()
            {
                RenderingMode =
                    openGlPreferred
                        ? [Win32RenderingMode.Wgl, Win32RenderingMode.Vulkan]
                        : [Win32RenderingMode.Vulkan, Win32RenderingMode.Wgl],
            })
            .With(new X11PlatformOptions()
            {
#pragma warning disable AVALONIA_X11_CSD
                EnableDrawnDecorations = true,
#pragma warning restore AVALONIA_X11_CSD
                RenderingMode =
                    openGlPreferred
                        ? [X11RenderingMode.Glx, X11RenderingMode.Vulkan]
                        : [X11RenderingMode.Vulkan, X11RenderingMode.Glx],
            })
            .With(new AvaloniaNativePlatformOptions()
            {
                RenderingMode = [AvaloniaNativeRenderingMode.OpenGl] // TODO: Metal
            })
            .With(new SkiaOptions()
            {
                MaxGpuResourceSizeBytes = 1024 * 600 * 4 * 12 * 4 // quadruple the default size
            })
            .With(new VulkanOptions()
            {
                VulkanDeviceCreationOptions = new VulkanDeviceCreationOptions
                {
                    PreferDiscreteGpu = true,
                },
            })
            .WithDeveloperTools()
            .WithDrawie(() => RebootWithOpengl(openGlPreferred))
            .LogToTrace();
    }

    private static void RebootWithOpengl(bool openGlPreferred)
    {
        if (openGlPreferred)
        {
            Console.WriteLine($"OpenGL driver failed, something could be corrupted. You can try running with Vulkan backend by setting 'Vulkan' inside {RenderApiPreferenceManager.ConfigPath} (omit apostrophe)");
            return;
        }

        RenderApiPreferenceManager.UpdateRenderApiPreference("OpenGL");
        
        var process = Process.GetCurrentProcess().MainModule.FileName;
        var createdProc = Process.Start(new ProcessStartInfo(process)
        {
            UseShellExecute = true
        });

        createdProc.WaitForExitAsync().ContinueWith(_ =>
        {
            Environment.Exit(0);
        });
    }
}
