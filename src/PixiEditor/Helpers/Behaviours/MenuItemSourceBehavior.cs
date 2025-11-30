using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Xaml.Interactivity;
using PixiEditor.UI.Common.Controls;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Helpers.Behaviours;

public class MenuItemSourceBehavior : Behavior
{
    public static readonly StyledProperty<ObservableCollection<string>> CheckedItemsProperty =
        AvaloniaProperty.Register<MenuItemSourceBehavior, ObservableCollection<string>>("CheckedItems");

    public static readonly StyledProperty<ObservableCollection<string>> ItemsSourceProperty =
        AvaloniaProperty.Register<MenuItemSourceBehavior, ObservableCollection<string>>("ItemsSource");

    public static readonly StyledProperty<Control> AdditionalElementProperty =
        AvaloniaProperty.Register<MenuItemSourceBehavior, Control>(
            nameof(AdditionalElement));

    public Control AdditionalElement
    {
        get => GetValue(AdditionalElementProperty);
        set => SetValue(AdditionalElementProperty, value);
    }

    public ObservableCollection<string> ItemsSource
    {
        get { return (ObservableCollection<string>)GetValue(ItemsSourceProperty); }
        set { SetValue(ItemsSourceProperty, value); }
    }

    public ObservableCollection<string> CheckedItems
    {
        get { return (ObservableCollection<string>)GetValue(CheckedItemsProperty); }
        set { SetValue(CheckedItemsProperty, value); }
    }

    static MenuItemSourceBehavior()
    {
        ItemsSourceProperty.Changed.AddClassHandler<MenuItemSourceBehavior>((behavior, args) =>
        {
            UpdateItems(behavior);
        });

        CheckedItemsProperty.Changed.AddClassHandler<MenuItemSourceBehavior>((behavior, args) =>
        {
            UpdateItems(behavior);
        });
    }

    private static void UpdateItems(MenuItemSourceBehavior behavior)
    {
        if (behavior.AssociatedObject is MenuItem menuItem)
        {
            if (behavior.ItemsSource == null)
                return;

            menuItem.Items.Clear();
            if (behavior.AdditionalElement != null)
            {
                menuItem.Items.Add(behavior.AdditionalElement);
                menuItem.Items.Add(new Separator());
            }

            foreach (var item in behavior.ItemsSource)
            {
                var subItem = new ToggleableMenuItem();
                Translator.SetKey(subItem, item);

                subItem.IsChecked = behavior.CheckedItems != null && behavior.CheckedItems.Contains(item);
                subItem.Click += (_, _) =>
                {
                    if (behavior.CheckedItems == null)
                        return;

                    if (subItem.IsChecked)
                    {
                        if (!behavior.CheckedItems.Contains(item))
                            behavior.CheckedItems.Add(item);
                    }
                    else
                    {
                        if (behavior.CheckedItems.Contains(item))
                            behavior.CheckedItems.Remove(item);
                    }
                };

                menuItem.Items.Add(subItem);
            }
        }
    }

    protected override void OnAttached()
    {
    }
}
