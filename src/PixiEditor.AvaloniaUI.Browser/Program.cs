using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using PixiEditor.AvaloniaUI;

[assembly: SupportedOSPlatform("browser")]

internal partial class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            await BuildAvaloniaApp()
                .WithInterFont()
                .StartBrowserAppAsync("out");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
