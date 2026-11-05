using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PixiEditor.ViewModels.Layers;

namespace PixiEditor.Views.Dialogs;

public partial class FiltersPopup : PixiEditorPopup
{
    public FiltersPopup()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Dispatcher.Post(() =>
        {
            AddFilterButton.Flyout.IsOpen = true;
        }, DispatcherPriority.Input);
    }

    private void Visual_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is CheckBox checkBox)
        {
            checkBox.AddHandler(Button.PointerPressedEvent, OnClick, RoutingStrategies.Bubble, true);
        }
    }

    private void OnClick(object? sender, RoutedEventArgs e)
    {
        CheckBox checkBox = (sender as CheckBox);
        checkBox.IsChecked = !checkBox.IsChecked;
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var first = ItemsControl?.ItemsPanelRoot?.Children?.FirstOrDefault();
            var vm = DataContext as FiltersPopupViewModel;
            if (first != null && vm != null)
            {
                var metadata = (first as ContentPresenter)?.Content as FilterMetadata;
                vm.AddFilter(metadata);
                (sender as Control).FindLogicalAncestorOfType<Popup>()?.IsOpen = false;
            }
        }
    }
}

