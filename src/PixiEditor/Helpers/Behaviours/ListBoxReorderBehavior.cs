using System.Collections.ObjectModel;
using Avalonia.Animation;
using Avalonia.Media;

namespace PixiEditor.Helpers.Behaviours;

using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

public class ListBoxReorderBehavior<T> : Behavior<ListBox>
{
    private ListBoxItem? draggedItem;
    private Point startPoint;
    private bool isDragging;
    private int dropIndex = -1;

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject!.AddHandler(
            InputElement.PointerPressedEvent,
            OnPointerPressed,
            RoutingStrategies.Tunnel);

        AssociatedObject.AddHandler(
            InputElement.PointerMovedEvent,
            OnPointerMoved,
            RoutingStrategies.Tunnel);

        AssociatedObject.AddHandler(
            InputElement.PointerReleasedEvent,
            OnPointerReleased,
            RoutingStrategies.Tunnel);
    }

    protected override void OnDetaching()
    {
        AssociatedObject!.RemoveHandler(
            InputElement.PointerPressedEvent,
            OnPointerPressed);

        AssociatedObject.RemoveHandler(
            InputElement.PointerMovedEvent,
            OnPointerMoved);

        AssociatedObject.RemoveHandler(
            InputElement.PointerReleasedEvent,
            OnPointerReleased);

        base.OnDetaching();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(AssociatedObject).Properties.PointerUpdateKind
            != PointerUpdateKind.LeftButtonPressed)
            return;

        var handle = e.Source as Control;

        draggedItem = handle.FindAncestorOfType<ListBoxItem>();

        if (draggedItem == null)
            return;

        startPoint = e.GetPosition(AssociatedObject);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (draggedItem == null)
            return;

        var position = e.GetPosition(AssociatedObject);

        if (!isDragging)
        {
            if (Math.Abs(position.Y - startPoint.Y) < 5)
                return;

            isDragging = true;
            e.Pointer.Capture(AssociatedObject);
            AssociatedObject.Cursor = new Cursor(StandardCursorType.DragMove);
        }

        UpdateInsertion(position);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (draggedItem == null)
            return;

        if (isDragging)
            Reorder();

        ClearInsertion();

        draggedItem = null;
        isDragging = false;
        dropIndex = -1;
        AssociatedObject.Cursor = new Cursor(StandardCursorType.Arrow);

        e.Pointer.Capture(null);
    }

    private void ClearInsertion()
    {
        foreach (var border in AssociatedObject!
                     .GetVisualDescendants()
                     .OfType<Border>()
                     .Where(x => x.Classes.Contains("reorder-placeholder")))
        {
            border.Classes.Remove("drop-before");
            border.Classes.Remove("drop-after");
        }
    }

    private void UpdateInsertion(Point position)
    {
        var listBox = AssociatedObject!;

        var itemContainers = listBox.GetVisualDescendants()
            .OfType<ListBoxItem>()
            .Where(x => x.DataContext != null)
            .OrderBy(x => x.TranslatePoint(
                new Point(0, 0), listBox)!.Value.Y)
            .ToList();

        foreach (var container in itemContainers)
        {
            var border = container.GetVisualDescendants()
                .OfType<Border>()
                .FirstOrDefault(x =>
                    x.Classes.Contains("reorder-placeholder"));

            border?.Classes.Remove("drop-before");
            border?.Classes.Remove("drop-after");
        }

        dropIndex = -1;

        foreach (var container in itemContainers)
        {
            if (container == draggedItem)
                continue;

            var top = container.TranslatePoint(
                new Point(0, 0), listBox)!.Value.Y;

            var middle = top + container.Bounds.Height / 2;

            if (position.Y < middle)
            {
                dropIndex = ((IList)listBox.ItemsSource!)
                    .IndexOf(container.DataContext);

                var border = container.GetVisualDescendants()
                    .OfType<Border>()
                    .FirstOrDefault(x =>
                        x.Classes.Contains("reorder-placeholder"));

                border?.Classes.Add("drop-before");
                return;
            }
        }

        if (itemContainers.Count == 0)
            return;

        var last = itemContainers.Last();

        dropIndex = ((IList)listBox.ItemsSource!)
            .IndexOf(last.DataContext) + 1;

        var lastBorder = last.GetVisualDescendants()
            .OfType<Border>()
            .FirstOrDefault(x =>
                x.Classes.Contains("reorder-placeholder"));

        lastBorder?.Classes.Add("drop-after");
    }

    private void Reorder()
    {
        var items = AssociatedObject!.ItemsSource as IList;

        if (items == null || draggedItem == null || dropIndex < 0)
            return;

        var oldIndex = items.IndexOf(draggedItem.DataContext);

        if (oldIndex < 0)
            return;

        var newIndex = dropIndex;

        if (oldIndex < newIndex)
            newIndex--;

        newIndex = Math.Clamp(newIndex, 0, items.Count - 1);

        if (oldIndex == newIndex)
            return;

        var item = items[oldIndex];

        if (items is ObservableCollection<T> observableCollection)
        {
            observableCollection.Move(oldIndex, newIndex);
        }
        else
        {
            items.RemoveAt(oldIndex);
            items.Insert(newIndex, item);
        }
    }
}
