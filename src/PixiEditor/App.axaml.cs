using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PixiEditor.Common.Performance;
using PixiEditor.Initialization;

namespace PixiEditor;

public partial class App : Application
{
    public override void Initialize()
    {
        using PerfMeasure _ = new(PerfEventType.AppInitialize);
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
            throw new NotImplementedException();
            //singleViewPlatform.MainView = new MainView { DataContext = new MainViewModel() };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
