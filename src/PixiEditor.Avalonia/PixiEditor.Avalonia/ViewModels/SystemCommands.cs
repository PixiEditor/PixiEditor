using System.Reactive;
using Avalonia.Controls;
using ReactiveUI;

namespace PixiEditor.Avalonia.ViewModels;

public static class SystemCommands
{
    public static ReactiveCommand<Window, Unit> CloseWindowCommand { get; } = ReactiveCommand.Create<Window>(CloseWindow);
    public static ReactiveCommand<Window, Unit> MaximizeWindowCommand { get; } = ReactiveCommand.Create<Window>(MaximizeWindow);
    public static ReactiveCommand<Window, Unit> MinimizeWindowCommand { get; } = ReactiveCommand.Create<Window>(MinimizeWindow);
    public static ReactiveCommand<Window, Unit> RestoreWindowCommand { get; } = ReactiveCommand.Create<Window>(RestoreWindow);

    public static void CloseWindow(Window window)
    {
        window.Close();
    }

    public static void MaximizeWindow(Window window)
    {
        window.WindowState = WindowState.Maximized;
    }

    public static void MinimizeWindow(Window window)
    {
        window.WindowState = WindowState.Minimized;
    }

    public static void RestoreWindow(Window window)
    {
        window.WindowState = WindowState.Normal;
    }
}
