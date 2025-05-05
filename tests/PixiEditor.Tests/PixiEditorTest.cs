using Drawie.Backend.Core.Bridge;
using Drawie.Numerics;
using Drawie.RenderApi.Vulkan;
using Drawie.Silk;
using Drawie.Skia;
using Drawie.Windowing;
using DrawiEngine;
using DrawiEngine.Desktop;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Extensions.Runtime;
using PixiEditor.Helpers;
using PixiEditor.Linux;
using PixiEditor.MacOs;
using PixiEditor.OperatingSystem;
using PixiEditor.Platform;
using PixiEditor.ViewModels;
using PixiEditor.Windows;

namespace PixiEditor.Tests;

public class PixiEditorTest
{
    public PixiEditorTest()
    {
        if (DrawingBackendApi.HasBackend)
        {
            return;
        }

        try
        {
            var engine = DesktopDrawingEngine.CreateDefaultDesktop();
            var app = new TestingApp();
            Console.WriteLine("Running DrawieEngine with configuration:");
            Console.WriteLine($"\t- RenderApi: {engine.RenderApi}");
            Console.WriteLine($"\t- WindowingPlatform: {engine.RenderApi}");
            Console.WriteLine($"\t- DrawingBackend: {engine.RenderApi}");

            app.Initialize(engine);
            IWindow window = app.CreateMainWindow();

            window.Initialize();

            DrawingBackendApi.InitializeBackend(engine.RenderApi);

            app.Run();
        }
        catch (Exception ex)
        {
            if (!DrawingBackendApi.HasBackend)
                DrawingBackendApi.SetupBackend(new SkiaDrawingBackend(), new DrawieRenderingDispatcher());
        }
    }
}

public class FullPixiEditorTest : PixiEditorTest
{
    public FullPixiEditorTest()
    {
        ExtensionLoader loader = new ExtensionLoader("TestExtensions", "TestExtensions/Unpacked");

        if (IOperatingSystem.Current == null)
        {
            IOperatingSystem os;
            if (System.OperatingSystem.IsWindows())
            {
                os = new WindowsOperatingSystem();
            }
            else if (System.OperatingSystem.IsLinux())
            {
                os = new LinuxOperatingSystem();
            }
            else if (System.OperatingSystem.IsMacOS())
            {
                os = new MacOperatingSystem();
            }
            else
            {
                throw new NotSupportedException("Unsupported operating system");
            }

            IOperatingSystem.RegisterOS(os);
        }

        if (IPlatform.Current == null)
        {
            IPlatform.RegisterPlatform(new TestPlatform());
        }

        var services = new ServiceCollection()
            .AddPlatform()
            .AddPixiEditor(loader)
            .AddExtensionServices(loader)
            .BuildServiceProvider();


        var vm = services.GetRequiredService<ViewModelMain>();
        vm.Setup(services);
    }

    class TestPlatform : IPlatform
    {
        public string Id { get; } = "TestPlatform";
        public string Name { get; } = "Tests";

        public bool PerformHandshake()
        {
            return true;
        }

        public void Update()
        {
        }

        public IAdditionalContentProvider? AdditionalContentProvider { get; } = new NullAdditionalContentProvider();
    }
}

public class TestingApp : DrawieApp
{
    IWindow window;

    public override IWindow CreateMainWindow()
    {
        window = Engine.WindowingPlatform.CreateWindow("Testing app", VecI.One);
        return window;
    }

    protected override void OnInitialize()
    {
        window.IsVisible = false;
    }
}