using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace PixiEditor.Helpers.Behaviours;

public class PrimaryPointerPressedTriggerBehavior : EventTriggerBase
{
    public static readonly StyledProperty<ICommand> CommandProperty = AvaloniaProperty.Register<PrimaryPointerPressedTriggerBehavior, ICommand>(
        nameof(Command));

    public static readonly StyledProperty<object> CommandParameterProperty = AvaloniaProperty.Register<PrimaryPointerPressedTriggerBehavior, object>(
        nameof(CommandParameter));

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public ICommand Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is InputElement inputElement)
        {
            inputElement.PointerPressed += OnPointerPressed;
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint((InputElement)sender).Properties.IsLeftButtonPressed)
        {
            if (Command?.CanExecute(CommandParameter) ?? false)
            {
                Command?.Execute(CommandParameter);
            }
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject is InputElement inputElement)
        {
            inputElement.PointerPressed -= OnPointerPressed;
        }
    }
}
