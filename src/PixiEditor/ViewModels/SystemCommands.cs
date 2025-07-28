using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;

namespace PixiEditor.ViewModels;

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

    public static ICommand CloseWindowCommand { get; } = new RelayCommand<Window>(CloseWindow);

    public static void CloseWindow(Window? obj)
    {
        obj?.Close();
    }
}
