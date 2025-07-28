using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Xaml.Interactivity;

namespace PixiEditor.Helpers.Behaviours;

public class SliderBindingBehavior : Behavior<Slider>
{
    public static readonly StyledProperty<bool> CanBindProperty = AvaloniaProperty.Register<SliderBindingBehavior, bool>(
        nameof(CanBind));

    public static readonly StyledProperty<IBinding?> ValueBindingProperty = AvaloniaProperty.Register<SliderBindingBehavior, IBinding?>(
        nameof(ValueBinding));

    [AssignBinding]
    public IBinding? ValueBinding
    {
        get => GetValue(ValueBindingProperty);
        set => SetValue(ValueBindingProperty, value);
    }

    public bool CanBind
    {
        get => GetValue(CanBindProperty);
        set => SetValue(CanBindProperty, value);
    }

    static SliderBindingBehavior()
    {
        CanBindProperty.Changed.Subscribe(OnCanBindChanged);
        ValueBindingProperty.Changed.Subscribe(OnBindingChanged);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject != null)
        {
            if (CanBind)
            {
                AssociatedObject.Bind(
                    RangeBase.ValueProperty,
                    ValueBinding);
            }
        }
    }

    private static void OnCanBindChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is SliderBindingBehavior behavior && behavior.AssociatedObject != null)
        {
            if (e.NewValue.Value && behavior.ValueBinding != null)
            {
                behavior.AssociatedObject.Bind(
                    RangeBase.ValueProperty,
                    behavior.ValueBinding);
            }
            else
            {
                behavior.AssociatedObject.ClearValue(RangeBase.ValueProperty);
            }
        }
    }

    private static void OnBindingChanged(AvaloniaPropertyChangedEventArgs<IBinding> e)
    {
        if (e.Sender is SliderBindingBehavior behavior && behavior.AssociatedObject != null)
        {
            if (behavior.CanBind && e.NewValue.Value != null)
            {
                behavior.AssociatedObject.Bind(
                    RangeBase.ValueProperty,
                    e.NewValue.Value);
            }
            else
            {
                behavior.AssociatedObject.ClearValue(RangeBase.ValueProperty);
            }
        }
    }
}
