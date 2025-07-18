using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Xaml.Interactivity;
using PixiEditor.Helpers.UI;

namespace PixiEditor.Helpers.Behaviours;

internal class ShowFlyoutOnTrigger : Behavior<Button>
{
    public static readonly StyledProperty<ExecutionTrigger> TriggerProperty =
        AvaloniaProperty.Register<ShowFlyoutOnTrigger, ExecutionTrigger>(
            nameof(Trigger));

    public ExecutionTrigger Trigger
    {
        get => GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }

    static ShowFlyoutOnTrigger()
    {
        TriggerProperty.Changed.AddClassHandler<ShowFlyoutOnTrigger, ExecutionTrigger>(TriggerChanged);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        if (Trigger != null)
        {
            Trigger.Triggered += OnTrigger;
        }
    }


    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (Trigger != null)
        {
            Trigger.Triggered -= OnTrigger;
        }
    }

    private void OnTrigger(object? sender, EventArgs e)
    {
        AssociatedObject?.Flyout?.ShowAt(AssociatedObject);
    }

    private static void TriggerChanged(ShowFlyoutOnTrigger sender, AvaloniaPropertyChangedEventArgs<ExecutionTrigger> e)
    {
        if (e.OldValue.Value != null)
        {
            e.OldValue.Value.Triggered -= sender.OnTrigger;
        }

        if (e.NewValue.Value != null)
        {
            e.NewValue.Value.Triggered += sender.OnTrigger;
        }
    }
}
