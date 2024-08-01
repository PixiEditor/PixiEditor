using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace PixiEditor.Helpers.Behaviours;
#nullable enable
internal class SliderUpdateBehavior : Behavior<Slider>
{
    public static readonly StyledProperty<double> BindingProperty
        = AvaloniaProperty.Register<SliderUpdateBehavior, double>(nameof(Binding));

    public double Binding
    {
        get => GetValue(BindingProperty);
        set => SetValue(BindingProperty, value);
    }

    public static readonly StyledProperty<ICommand> DragValueChangedProperty = AvaloniaProperty.Register<SliderUpdateBehavior, ICommand>(
        nameof(DragValueChanged));

    public ICommand DragValueChanged
    {
        get => GetValue(DragValueChangedProperty);
        set => SetValue(DragValueChangedProperty, value);
    }

    public static readonly StyledProperty<ICommand> DragEndedProperty = AvaloniaProperty.Register<SliderUpdateBehavior, ICommand>(
        nameof(DragEnded));

    public ICommand DragEnded
    {
        get => GetValue(DragEndedProperty);
        set => SetValue(DragEndedProperty, value);
    }

    public static readonly StyledProperty<ICommand> DragStartedProperty = AvaloniaProperty.Register<SliderUpdateBehavior, ICommand>(
        nameof(DragStarted));

    public ICommand DragStarted
    {
        get => GetValue(DragStartedProperty);
        set => SetValue(DragStartedProperty, value);
    }

    public static readonly StyledProperty<ICommand> SetValueCommandProperty = AvaloniaProperty.Register<SliderUpdateBehavior, ICommand>(
        nameof(SetValueCommand));

    public ICommand SetValueCommand
    {
        get => GetValue(SetValueCommandProperty);
        set => SetValue(SetValueCommandProperty, value);
    }

    public static readonly StyledProperty<double> ValueFromSliderProperty = AvaloniaProperty.Register<SliderUpdateBehavior, double>(
        nameof(ValueFromSlider));
    public double ValueFromSlider
    {
        get => (double)GetValue(ValueFromSliderProperty);
        set => SetValue(ValueFromSliderProperty, value);
    }

    static SliderUpdateBehavior()
    {
        BindingProperty.Changed.Subscribe(OnBindingValuePropertyChange);
        ValueFromSliderProperty.Changed.Subscribe(OnSliderValuePropertyChange);
    }

    private bool attached = false;
    private bool dragging = false;

    private bool bindingValueChangedWhileDragging = false;
    private double bindingValueWhileDragging = 0.0;

    private bool skipSetValue;
    
    protected override void OnAttached()
    {
        AssociatedObject.Loaded += AssociatedObject_Loaded;
        AssociatedObject.Focusable = false;


        if (AssociatedObject.IsLoaded)
            AttachEvents();
    }

    private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
    {
        AttachEvents();
    }

    private void AttachEvents()
    {
        if (attached)
            return;

        Thumb? thumb = GetThumb(AssociatedObject);
        if (thumb is null)
            return;

        attached = true;

        thumb.DragStarted += Thumb_DragStarted;
        thumb.DragCompleted += Thumb_DragCompleted;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= AssociatedObject_Loaded;
        if (!attached)
            return;
        Thumb? thumb = GetThumb(AssociatedObject);
        if (thumb is null)
            return;

        thumb.DragStarted -= Thumb_DragStarted;
        thumb.DragCompleted -= Thumb_DragCompleted;
    }

    private static void OnSliderValuePropertyChange(AvaloniaPropertyChangedEventArgs<double> e)
    {
        SliderUpdateBehavior obj = (SliderUpdateBehavior)e.Sender;
        
        if (obj.dragging)
        {
            if (obj.DragValueChanged is not null && obj.DragValueChanged.CanExecute(e.NewValue.Value))
                obj.DragValueChanged.Execute(e.NewValue.Value);
        }
        else if (!obj.skipSetValue)
        {
            if (obj.SetValueCommand is not null && obj.SetValueCommand.CanExecute(e.NewValue.Value))
                obj.SetValueCommand.Execute(e.NewValue.Value);
        }
    }

    private static void OnBindingValuePropertyChange(AvaloniaPropertyChangedEventArgs<double> args)
    {
        SliderUpdateBehavior obj = (SliderUpdateBehavior)args.Sender;
        obj.skipSetValue = true;
        if (obj.dragging)
        {
            obj.bindingValueChangedWhileDragging = true;
            obj.bindingValueWhileDragging = args.NewValue.Value;
            obj.skipSetValue = false;
            return;
        }
        obj.ValueFromSlider = args.NewValue.Value;
        obj.skipSetValue = false;
    }

    private void Thumb_DragCompleted(object sender, VectorEventArgs e)
    {
        dragging = false;
        if (DragEnded is not null && DragEnded.CanExecute(null))
            DragEnded.Execute(null);
        if (bindingValueChangedWhileDragging)
            ValueFromSlider = bindingValueWhileDragging;
        bindingValueChangedWhileDragging = false;
    }

    private void Thumb_DragStarted(object sender, VectorEventArgs e)
    {
        dragging = true;
        if (DragStarted is not null && DragStarted.CanExecute(null))
            DragStarted.Execute(null);
    }

    private static Thumb? GetThumb(Slider slider)
    {
        /*Track? track = slider.Template.FindName("PART_Track", slider) as Track;
        return track is null ? null : track.Thumb;*/
        return slider.FindDescendantOfType<Thumb>();
    }


}
