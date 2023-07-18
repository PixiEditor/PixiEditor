using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;

namespace PixiEditor.Avalonia.ViewModels;

public static class SystemCommands
{
    public static RoutedEvent<RoutedEventArgs> CloseWindowEvent { get; }
        = RoutedEvent.Register<Window, RoutedEventArgs>(nameof(CloseWindowEvent), RoutingStrategies.Bubble);
    public static RoutedEvent<RoutedEventArgs> MaximizeWindowEvent { get; }
        = RoutedEvent.Register<Window, RoutedEventArgs>(nameof(MaximizeWindowEvent), RoutingStrategies.Bubble);
    public static RoutedEvent<RoutedEventArgs> MinimizeWindowEvent { get; }
        = RoutedEvent.Register<Window, RoutedEventArgs>(nameof(MinimizeWindowEvent), RoutingStrategies.Bubble);
    public static RoutedEvent<RoutedEventArgs> RestoreWindowEvent { get; }
        = RoutedEvent.Register<Window, RoutedEventArgs>(nameof(RestoreWindowEvent), RoutingStrategies.Bubble);

    public static RelayCommand<Window> CloseWindowCommand { get; }

    static SystemCommands()
    {
        CloseWindowCommand = new RelayCommand<Window>(CloseWindow);
    }

    private static void CloseWindow(Window? obj)
    {

    }
}
