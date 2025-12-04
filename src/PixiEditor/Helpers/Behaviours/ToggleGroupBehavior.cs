using Avalonia;
using Avalonia.Controls.Primitives;

namespace PixiEditor.Helpers.Behaviours;

public static class ToggleGroupBehavior
{
    // The group identifier
    public static readonly AttachedProperty<string?> GroupNameProperty =
        AvaloniaProperty.RegisterAttached<ToggleButton, string?>("GroupName", typeof(ToggleGroupBehavior));

    // The enum (or any value) this button represents
    public static readonly AttachedProperty<object?> ValueProperty =
        AvaloniaProperty.RegisterAttached<ToggleButton, object?>("Value", typeof(ToggleGroupBehavior));

    // The current selected value for the entire group (bind this!)
    public static readonly AttachedProperty<object?> SelectedValueProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, object?>("SelectedValue", typeof(ToggleGroupBehavior));

    private static readonly Dictionary<string, List<ToggleButton>> _groups = new();

    static ToggleGroupBehavior()
    {
        GroupNameProperty.Changed.AddClassHandler<ToggleButton>(OnGroupNameChanged);
        ValueProperty.Changed.AddClassHandler<ToggleButton>(OnValueChanged);
        SelectedValueProperty.Changed.AddClassHandler<AvaloniaObject>(OnSelectedValueChanged);
    }

    public static void SetGroupName(AvaloniaObject element, string? value) =>
        element.SetValue(GroupNameProperty, value);

    public static string? GetGroupName(AvaloniaObject element) =>
        element.GetValue(GroupNameProperty);

    public static void SetValue(AvaloniaObject element, object? value) =>
        element.SetValue(ValueProperty, value);

    public static object? GetValue(AvaloniaObject element) =>
        element.GetValue(ValueProperty);

    public static void SetSelectedValue(AvaloniaObject element, object? value) =>
        element.SetValue(SelectedValueProperty, value);

    public static object? GetSelectedValue(AvaloniaObject element) =>
        element.GetValue(SelectedValueProperty);

    private static void OnGroupNameChanged(ToggleButton button, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is string oldGroup && _groups.TryGetValue(oldGroup, out var oldList))
            oldList.Remove(button);

        if (e.NewValue is string newGroup)
        {
            if (!_groups.TryGetValue(newGroup, out var list))
            {
                list = new List<ToggleButton>();
                _groups[newGroup] = list;
            }

            list.Add(button);
            button.Checked += ButtonChecked;
            button.Click += ButtonClickPreventUntoggle;
        }
    }

    private static void OnValueChanged(ToggleButton button, AvaloniaPropertyChangedEventArgs e)
    {
        UpdateIsChecked(button);
    }

    private static void OnSelectedValueChanged(AvaloniaObject o, AvaloniaPropertyChangedEventArgs e)
    {
        // Find all buttons with matching group and update their IsChecked
        foreach (var group in _groups)
        {
            foreach (var btn in group.Value)
            {
                UpdateIsChecked(btn);
            }
        }
    }

    private static void ButtonChecked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is ToggleButton btn)
        {
            var group = GetGroupName(btn);
            var value = GetValue(btn);

            // Set group's SelectedValue to this button's value
            if (group is not null)
            {
                foreach (var kv in _groups)
                {
                    foreach (var b in kv.Value)
                    {
                        if (b != btn && GetGroupName(b) == group)
                            b.IsChecked = false;
                    }
                }
            }

            // Update bound SelectedValue on parent DataContext
            SetSelectedValue(btn, value);
        }
    }

    private static void ButtonClickPreventUntoggle(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is ToggleButton btn &&
            btn.IsChecked == false &&
            Equals(GetSelectedValue(btn), GetValue(btn)))
        {
            // Prevent unchecking current selection
            btn.IsChecked = true;
            e.Handled = true;
        }
    }

    private static void UpdateIsChecked(ToggleButton btn)
    {
        var selected = GetSelectedValue(btn);
        var value = GetValue(btn);
        btn.IsChecked = Equals(selected, value);
    }
}
