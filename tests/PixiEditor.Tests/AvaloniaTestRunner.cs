using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Platform;
using Drawie.Interop.VulkanAvalonia;
using PixiEditor.Tests;
using Xunit.Sdk;
using Xunit.v3;

[assembly:AvaloniaTestFramework]
[assembly:CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true, MaxParallelThreads = 1)]
[assembly: AvaloniaTestApplication(typeof(AvaloniaTestRunner))]
namespace PixiEditor.Tests
{
    public class AvaloniaTestRunner
    {
        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions()
            {
                UseHeadlessDrawing = true,
                FrameBufferFormat = PixelFormat.Rgba8888,
            }).WithDrawie();
    }
}
