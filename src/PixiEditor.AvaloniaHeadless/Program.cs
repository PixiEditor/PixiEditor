using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Platform;
using Avalonia.Threading;
using PixiEditor.AvaloniaHeadless;
using PixiEditor.AvaloniaUI;
using PixiEditor.AvaloniaUI.Models.IO;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Skia;
using PixiEditor.Extensions.Runtime;
using PixiEditor.OperatingSystem;
using PixiEditor.Platform;
using Silk.NET.GLFW;
using SkiaSharp;
using Window = Avalonia.Controls.Window;

public static class Program
{
    public static void Main(string[] args)
    {
        AppWindow window = new AppWindow();
        window.Init += (gr) =>
        {
            WindowOnInit(window.Dispatcher, gr, args);
        };

        window.Render += WindowOnRender;

        window.Run();
    }

    private static void WindowOnRender(SKSurface arg1, double arg2)
    {
    }

    private static void WindowOnInit(Func<Delegate, string[], object> dispatcher, GRContext grContext, string[] args)
    
    {
        SkiaDrawingBackend skiaDrawingBackend = new SkiaDrawingBackend(grContext);
        DrawingBackendApi.SetupBackend(skiaDrawingBackend);

        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        DrawingBackendApi.Current.Dispatcher = dispatcher;

        var appBuilder = AppBuilder.Configure<App>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions() { UseHeadlessDrawing = false });
        

        appBuilder.StartWithClassicDesktopLifetime(args);
        
        //appBuilder.Start(AppMain, args);
    }

    private static void AppMain(Application app, string[] args)
    {
        Type type = App.MainWindowType;
        ExtensionLoader loader = InitApp();

        var mainWindow = (Window)Activator.CreateInstance(type, loader);
        
        mainWindow.Show();
    }

    private static ExtensionLoader InitApp()
    {
        //LoadingWindow.ShowInNewThread();

        InitPlatform();

        ExtensionLoader extensionLoader = new ExtensionLoader(Paths.ExtensionPackagesPath, Paths.UserExtensionsPath);
        //TODO: fetch from extension store
        extensionLoader.AddOfficialExtension("pixieditor.supporterpack",
            new OfficialExtensionData("supporter-pack.snk", AdditionalContentProduct.SupporterPack));
        extensionLoader.LoadExtensions();

        return extensionLoader;
    }


    private static void InitPlatform()
    {
        var platform = GetActivePlatform();
        IPlatform.RegisterPlatform(platform);
        platform.PerformHandshake();
    }

    private static IPlatform GetActivePlatform()
    {
#if STEAM
            return new PixiEditor.Platform.Steam.SteamPlatform();
#elif MSIX || MSIX_DEBUG
            return new PixiEditor.Platform.MSStore.MicrosoftStorePlatform();
#else
        return new PixiEditor.Platform.Standalone.StandalonePlatform();
#endif
    }

    private static void InitOperatingSystem()
    {
        IOperatingSystem.RegisterOS(GetActiveOperatingSystem());
    }

    private static IOperatingSystem GetActiveOperatingSystem()
    {
#if WINDOWS
        return new PixiEditor.Windows.WindowsOperatingSystem();
#elif LINUX
            return new PixiEditor.Linux.LinuxOperatingSystem();
#elif MACOS
            return new PixiEditor.MacOs.MacOperatingSystem();
#else
            throw new PlatformNotSupportedException("This platform is not supported");
#endif
    }
}
