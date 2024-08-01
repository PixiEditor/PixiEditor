using Avalonia.Input;
using Avalonia.Interactivity;

namespace PixiEditor.Helpers.UI;

public delegate void DragEventHandler(object sender, DragEventArgs e);

public static class DragDropEvents
{
    public static readonly RoutedEvent<DragEventArgs> DragEnterEvent =
        RoutedEvent.Register<DragEventArgs>(
            "DragEnter",
            RoutingStrategies.Bubble,
            typeof(DragDropEvents));


    public static readonly RoutedEvent<DragEventArgs> DragLeaveEvent =
        RoutedEvent.Register<DragEventArgs>(
            "DragLeave",
            RoutingStrategies.Bubble,
            typeof(DragDropEvents));


    public static readonly RoutedEvent<RoutedEventArgs> DragOverEvent =
        RoutedEvent.Register<RoutedEventArgs>(
            "DragOver",
            RoutingStrategies.Bubble,
            typeof(DragDropEvents));


    public static readonly RoutedEvent<DragEventArgs> DropEvent =
        RoutedEvent.Register<DragEventArgs>(
            "Drop",
            RoutingStrategies.Bubble,
            typeof(DragDropEvents));

    public static void AddDragEnterHandler(Interactive control, EventHandler<DragEventArgs> handler)
    {
        var checkedHandler = WithCheck(control, handler);
        control.AddHandler(
            DragEnterEvent,
            checkedHandler,
            RoutingStrategies.Bubble);

        control.AddHandler(
            DragDrop.DragEnterEvent,
            checkedHandler,
            RoutingStrategies.Bubble);
    }

    public static void AddDragLeaveHandler(Interactive control, EventHandler<DragEventArgs> handler)
    {
        var checkedHandler = WithCheck(control, handler);

        control.AddHandler(
            DragLeaveEvent,
            checkedHandler,
            RoutingStrategies.Bubble);

        control.AddHandler(
            DragDrop.DragLeaveEvent,
            checkedHandler,
            RoutingStrategies.Bubble);
    }

    public static void AddDragOverHandler(Interactive control, EventHandler<DragEventArgs> handler)
    {
        var checkedHandler = WithCheck(control, handler);

        control.AddHandler(
            DragOverEvent,
            checkedHandler,
            RoutingStrategies.Bubble);

        control.AddHandler(
            DragDrop.DragOverEvent,
            checkedHandler,
            RoutingStrategies.Bubble);
    }

    public static void AddDropHandler(Interactive control, EventHandler<DragEventArgs> handler)
    {
        var checkedHandler = WithCheck(control, handler);
        control.AddHandler(
            DropEvent,
            checkedHandler,
            RoutingStrategies.Bubble);

        control.AddHandler(
            DragDrop.DropEvent,
            checkedHandler,
            RoutingStrategies.Bubble);
    }

    public static void RemoveDragEnterHandler(Interactive control, EventHandler<DragEventArgs> handler)
    {
        control.RemoveHandler(
            DragEnterEvent,
            handler);

        control.RemoveHandler(
            DragDrop.DragEnterEvent,
            handler);
    }

    public static void RemoveDragLeaveHandler(Interactive control, EventHandler<DragEventArgs> handler)
    {
        control.RemoveHandler(
            DragLeaveEvent,
            handler);

        control.RemoveHandler(
            DragDrop.DragLeaveEvent,
            handler);
    }

    public static void RemoveDragOverHandler(Interactive control, EventHandler<DragEventArgs> handler)
    {
        control.RemoveHandler(
            DragOverEvent,
            handler);

        control.RemoveHandler(
            DragDrop.DragOverEvent,
            handler);
    }

    public static void RemoveDropHandler(Interactive control, EventHandler<DragEventArgs> handler)
    {
        control.RemoveHandler(
            DropEvent,
            handler);

        control.RemoveHandler(
            DragDrop.DropEvent,
            handler);
    }

    private static EventHandler<T> WithCheck<T>(object source, EventHandler<T> handler) where T : RoutedEventArgs
    {
        return (sender, args) =>
        {
            if (source == sender)
            {
                handler(sender, args);
            }
        };
    }
}
