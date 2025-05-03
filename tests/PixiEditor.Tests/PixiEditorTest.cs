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

        var engine = DesktopDrawingEngine.CreateDefaultDesktop();
        var app = new TestingApp();
        app.Initialize(engine);
        IWindow window = app.CreateMainWindow();
        window.IsVisible = false;
        window.Initialize();
        DrawingBackendApi.InitializeBackend(engine.RenderApi);
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
    public override IWindow CreateMainWindow()
    {
        return Engine.WindowingPlatform.CreateWindow("Testing app", VecI.One);
    }

    protected override void OnInitialize()
    {

    }
}