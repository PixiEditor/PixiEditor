using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
namespace PixiEditor.Helpers.Behaviours;
#nullable enable
internal class SliderUpdateBehavior : Behavior<Slider>
{
    public static readonly DependencyProperty BindingProperty =
        DependencyProperty.Register(nameof(Binding), typeof(double), typeof(SliderUpdateBehavior), new(0.0, OnBindingValuePropertyChange));

    public double Binding
    {
        get => (double)GetValue(BindingProperty);
        set => SetValue(BindingProperty, value);
    }


    public static DependencyProperty DragValueChangedProperty = DependencyProperty.Register(nameof(DragValueChanged), typeof(ICommand), typeof(SliderUpdateBehavior));
    public ICommand DragValueChanged
    {
        get => (ICommand)GetValue(DragValueChangedProperty);
        set => SetValue(DragValueChangedProperty, value);
    }

    public static DependencyProperty DragEndedProperty = DependencyProperty.Register(nameof(DragEnded), typeof(ICommand), typeof(SliderUpdateBehavior));
    public ICommand DragEnded
    {
        get => (ICommand)GetValue(DragEndedProperty);
        set => SetValue(DragEndedProperty, value);
    }

    public static readonly DependencyProperty DragStartedProperty =
        DependencyProperty.Register(nameof(DragStarted), typeof(ICommand), typeof(SliderUpdateBehavior), new(null));

    public ICommand DragStarted
    {
        get => (ICommand)GetValue(DragStartedProperty);
        set => SetValue(DragStartedProperty, value);
    }

    public static readonly DependencyProperty SetOpacityProperty =
        DependencyProperty.Register(nameof(SetOpacity), typeof(ICommand), typeof(SliderUpdateBehavior), new(null));

    public ICommand SetOpacity
    {
        get => (ICommand)GetValue(SetOpacityProperty);
        set => SetValue(SetOpacityProperty, value);
    }

    public static DependencyProperty ValueFromSliderProperty =
        DependencyProperty.Register(nameof(ValueFromSlider), typeof(double), typeof(SliderUpdateBehavior), new(OnSliderValuePropertyChange));
    public double ValueFromSlider
    {
        get => (double)GetValue(ValueFromSliderProperty);
        set => SetValue(ValueFromSliderProperty, value);
    }

    private bool attached = false;
    private bool dragging = false;

    private bool bindingValueChangedWhileDragging = false;
    private double bindingValueWhileDragging = 0.0;

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

    private static void OnSliderValuePropertyChange(DependencyObject slider, DependencyPropertyChangedEventArgs e)
    {
        SliderUpdateBehavior obj = (SliderUpdateBehavior)slider;
        if (obj.dragging)
        {
            if (obj.DragValueChanged is not null && obj.DragValueChanged.CanExecute(e.NewValue))
                obj.DragValueChanged.Execute(e.NewValue);
        }
        else
        {
            if (obj.SetOpacity is not null && obj.SetOpacity.CanExecute(e.NewValue))
                obj.SetOpacity.Execute(e.NewValue);
        }
    }

    private static void OnBindingValuePropertyChange(DependencyObject slider, DependencyPropertyChangedEventArgs e)
    {
        SliderUpdateBehavior obj = (SliderUpdateBehavior)slider;
        if (obj.dragging)
        {
            obj.bindingValueChangedWhileDragging = true;
            obj.bindingValueWhileDragging = (double)e.NewValue;
            return;
        }
        obj.ValueFromSlider = (double)e.NewValue;
    }

    private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        dragging = false;
        if (DragEnded is not null && DragEnded.CanExecute(null))
            DragEnded.Execute(null);
        if (bindingValueChangedWhileDragging)
            ValueFromSlider = bindingValueWhileDragging;
        bindingValueChangedWhileDragging = false;
    }

    private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
    {
        dragging = true;
        if (DragStarted is not null && DragStarted.CanExecute(null))
            DragStarted.Execute(null);
    }

    private static Thumb? GetThumb(Slider slider)
    {
        Track? track = slider.Template.FindName("PART_Track", slider) as Track;
        return track is null ? null : track.Thumb;
    }


}
