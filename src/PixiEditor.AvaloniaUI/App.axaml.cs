using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Initialization;
using PixiEditor.AvaloniaUI.ViewModels;
using PixiEditor.AvaloniaUI.Views;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Skia;
using PixiEditor.Platform;

namespace PixiEditor.AvaloniaUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ClassicDesktopEntry entry = new(desktop);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            var services = new ServiceCollection()
                // .AddPlatform()
                .AddPixiEditor(null);

            //throw new NotImplementedException();

            SkiaDrawingBackend skiaDrawingBackend = new SkiaDrawingBackend();
            DrawingBackendApi.SetupBackend(skiaDrawingBackend);

            var viewModelMain = new ViewModelMain();

            services.AddSingleton(viewModelMain);
            
            viewModelMain.Setup(services.BuildServiceProvider());
            
            singleViewPlatform.MainView = new MainView { DataContext = viewModelMain };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
