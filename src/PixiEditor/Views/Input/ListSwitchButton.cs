using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PixiEditor.Views.Input;

internal class ListSwitchButton : Button, INotifyPropertyChanged
{
    public static readonly StyledProperty<ObservableCollection<SwitchItem>> ItemsProperty =
        AvaloniaProperty.Register<ListSwitchButton, ObservableCollection<SwitchItem>>(nameof(Items));

    public static readonly StyledProperty<SwitchItem> ActiveItemProperty =
        AvaloniaProperty.Register<ListSwitchButton, SwitchItem>(nameof(ActiveItem));

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<SwitchItem> Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public SwitchItem ActiveItem
    {
        get => GetValue(ActiveItemProperty);
        set => SetValue(ActiveItemProperty, value);
    }

    static ListSwitchButton()
    {
        ItemsProperty.Changed.Subscribe(CollChanged);
    }

    public ListSwitchButton()
    {
        Click += ListSwitchButton_Click;
    }

    private static void CollChanged(AvaloniaPropertyChangedEventArgs<ObservableCollection<SwitchItem>> e)
    {
        ListSwitchButton button = (ListSwitchButton)e.Sender;

        ObservableCollection<SwitchItem> oldVal = e.OldValue.Value;
        ObservableCollection<SwitchItem> newVal = e.NewValue.Value;
        if ((oldVal == null || oldVal.Count == 0) && newVal != null && newVal.Count > 0)
        {
            button.ActiveItem = newVal[0];
        }
    }

    private void ListSwitchButton_Click(object sender, RoutedEventArgs e)
    {
        if (!Items.Contains(ActiveItem))
        {
            throw new ArgumentException("Items doesn't contain specified Item.");
        }

        int index = Items.IndexOf(ActiveItem) + 1;
        if (index > Items.Count - 1)
        {
            index = 0;
        }
        ActiveItem = Items[Math.Clamp(index, 0, Items.Count - 1)];
    }
}
