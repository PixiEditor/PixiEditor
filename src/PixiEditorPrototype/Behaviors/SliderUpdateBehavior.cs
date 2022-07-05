using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace PixiEditorPrototype.Behaviors;

internal class SliderUpdateBehavior : Behavior<Slider>
{
    public static DependencyProperty DragValueChangedProperty = DependencyProperty.Register(nameof(DragValueChanged), typeof(ICommand), typeof(SliderUpdateBehavior));
    public ICommand? DragValueChanged
    {
        get => (ICommand)GetValue(DragValueChangedProperty);
        set => SetValue(DragValueChangedProperty, value);
    }
    public static DependencyProperty DragEndedProperty = DependencyProperty.Register(nameof(DragEnded), typeof(ICommand), typeof(SliderUpdateBehavior));
    public ICommand? DragEnded
    {
        get => (ICommand)GetValue(DragEndedProperty);
        set => SetValue(DragEndedProperty, value);
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
    private bool valueChangedWhileDragging = false;
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
        attached = true;
        var thumb = GetThumb(AssociatedObject);
        if (thumb is null)
            return;

        thumb.DragStarted += Thumb_DragStarted;
        thumb.DragCompleted += Thumb_DragCompleted;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= AssociatedObject_Loaded;
        if (!attached)
            return;
        var thumb = GetThumb(AssociatedObject);
        if (thumb is null)
            return;

        thumb.DragStarted -= Thumb_DragStarted;
        thumb.DragCompleted -= Thumb_DragCompleted;
    }

    private static void OnSliderValuePropertyChange(DependencyObject slider, DependencyPropertyChangedEventArgs e)
    {
        var obj = (SliderUpdateBehavior)slider;
        if (obj.dragging)
        {
            if (obj.DragValueChanged is not null && obj.DragValueChanged.CanExecute(e.NewValue))
                obj.DragValueChanged.Execute(e.NewValue);
            obj.valueChangedWhileDragging = true;
        }
    }

    private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        dragging = false;
        if (valueChangedWhileDragging && DragEnded is not null && DragEnded.CanExecute(null))
            DragEnded.Execute(null);
        valueChangedWhileDragging = false;
    }

    private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
    {
        dragging = true;
    }

    private static Thumb? GetThumb(Slider slider)
    {
        var track = slider.Template.FindName("PART_Track", slider) as Track;
        return track is null ? null : track.Thumb;
    }


}
