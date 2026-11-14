using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PixiEditor.ViewModels.Layers;

namespace PixiEditor.Views.Dialogs.Filters;

public partial class FilterPicker : UserControl
{
    public FilterPicker()
    {
        InitializeComponent();
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not FiltersPopupViewModel vm) return;

        if (e.Key == Key.Enter)
        {
            var first = ItemsControl?.ItemsPanelRoot?.Children?.FirstOrDefault();
            if (first != null || vm.SelectedPickerItem != null)
            {
                var metadata = vm.SelectedPickerItem ?? (first as ContentPresenter)?.Content as FilterMetadata;
                vm.AddFilter(metadata);
                (sender as Control).FindLogicalAncestorOfType<Button>()?.Flyout.Hide();
            }
        }
        else if (e.Key is Key.Down or Key.Up)
        {
            if (vm.FilteredFilters.Count == 0) return;
            int mod = e.Key == Key.Down ? 1 : -1;
            int currentIndex = vm.FilteredFilters.IndexOf(vm.SelectedPickerItem);
            var item = ItemsControl?.ItemsPanelRoot?.Children?.ElementAtOrDefault(currentIndex);
            item?.FindLogicalDescendantOfType<Button>().Classes.Remove("selected");

            int newIndex = currentIndex + mod;
            if (newIndex < 0)
            {
                newIndex = vm.FilteredFilters.Count - 1;
            }
            else if (newIndex >= vm.FilteredFilters.Count)
            {
                newIndex = 0;
            }

            vm.SelectedPickerItem = vm.FilteredFilters[newIndex];
            item = ItemsControl?.ItemsPanelRoot?.Children?.ElementAtOrDefault(newIndex);
            item?.FindLogicalDescendantOfType<Button>().Classes.Add("selected");

            var scrollViewer = item.FindLogicalAncestorOfType<ScrollViewer>();

            if (scrollViewer != null)
            {
                var itemTopLeft = item.TranslatePoint(default, scrollViewer);
                var itemBottomRight = item.TranslatePoint(
                    new Point(item.Bounds.Width, item.Bounds.Height),
                    scrollViewer);

                if (itemTopLeft is { } topLeft && itemBottomRight is { } bottomRight)
                {
                    if (topLeft.Y < 0)
                    {
                        scrollViewer.Offset = scrollViewer.Offset.WithY(
                            scrollViewer.Offset.Y + topLeft.Y);
                    }
                    else if (bottomRight.Y > scrollViewer.Viewport.Height)
                    {
                        scrollViewer.Offset = scrollViewer.Offset.WithY(
                            scrollViewer.Offset.Y + bottomRight.Y - scrollViewer.Viewport.Height);
                    }
                }
            }
        }
    }

    private void Button_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        Button btn = sender as Button;
        FiltersPopupViewModel vm = DataContext as FiltersPopupViewModel;
        if (btn != null && vm != null)
        {
            btn.Classes.Set("selected", (btn.DataContext as FilterMetadata) == vm.SelectedPickerItem);
        }
    }

    private void Search_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        FiltersPopupViewModel vm = DataContext as FiltersPopupViewModel;
        vm?.SearchText = "";
    }
}
