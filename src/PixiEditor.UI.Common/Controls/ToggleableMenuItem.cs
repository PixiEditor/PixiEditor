using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using PixiEditor.UI.Common.Extensions;

namespace PixiEditor.UI.Common.Controls;

[PseudoClasses(":checked")]
public class ToggleableMenuItem : MenuItem
{
    public static readonly StyledProperty<bool> IsCheckedProperty = AvaloniaProperty.Register<ToggleableMenuItem, bool>(
        nameof(IsChecked));

    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public ToggleableMenuItem()
    {
        IsCheckedProperty.Changed.Subscribe(OnCheckedChanged);
        PointerPressed += OnPointerPressedHandler;
    }

    private void OnPointerPressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            IsChecked = !IsChecked;
        }
    }

    private void OnCheckedChanged(AvaloniaPropertyChangedEventArgs<bool> args)
    {
        if (args.NewValue.Value)
        {
            PseudoClasses.Add("checked");
        }
        else
        {
            PseudoClasses.Remove("checked");
        }
    }
}
