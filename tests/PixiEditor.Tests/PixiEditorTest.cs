using Drawie.Backend.Core.Bridge;
using Drawie.Skia;
using DrawiEngine;
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

        SkiaDrawingBackend skiaDrawingBackend = new SkiaDrawingBackend();
        DrawingBackendApi.SetupBackend(skiaDrawingBackend, new DrawieRenderingDispatcher());
    }
}

public class FullPixiEditorTest : PixiEditorTest
{
    public FullPixiEditorTest()
    {
        ExtensionLoader loader = new ExtensionLoader("TestExtensions", "TestExtensions/Unpacked");

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
        IPlatform.RegisterPlatform(new TestPlatform());

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