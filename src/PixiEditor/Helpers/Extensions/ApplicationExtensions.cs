using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace PixiEditor.Helpers.Extensions;

public static class ApplicationExtensions
{
    public static void ForDesktopMainWindow(this Application app, Action<Window> action)
    {
        switch (app.ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                action(desktop.MainWindow);
                break;
        }
    }

    public static async Task ForDesktopMainWindowAsync(this Application app, Func<Window, Task> action)
    {
        switch (app.ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                await action(desktop.MainWindow);
                break;
            default:
                throw new NotSupportedException("ApplicationLifetime is not supported");
        }
    }
}
