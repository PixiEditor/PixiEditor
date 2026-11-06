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
            if (DataContext is FiltersPopupViewModel vm)
            {
                if (vm.LayerFilters.Count == 0)
                {
                    AddFilterButton.Flyout.IsOpen = true;
                }
                else
                {
                    vm.SelectedFilter = vm.LayerFilters.FirstOrDefault();
                }
            }
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
}

