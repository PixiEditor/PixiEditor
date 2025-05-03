using Avalonia;
using Avalonia.Headless;
using Avalonia.Platform;
using Drawie.Interop.VulkanAvalonia;
using PixiEditor.Tests;

[assembly:TestFramework("PixiEditor.Tests.AvaloniaTestRunner", "PixiEditor.Tests")]
[assembly:CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = false, MaxParallelThreads = 1)]
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
            });
    }
}
