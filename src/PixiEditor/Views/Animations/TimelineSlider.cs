using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace PixiEditor.Views.Animations;

public class TimelineSlider : Slider
{
    public static readonly StyledProperty<double> ScaleProperty = AvaloniaProperty.Register<TimelineSlider, double>(
        nameof(Scale), 100);

    public static readonly StyledProperty<Vector> OffsetProperty = AvaloniaProperty.Register<TimelineSlider, Vector>(
        nameof(Offset), new Vector(0, 0));
    
    public static readonly StyledProperty<double> MinLeftOffsetProperty = AvaloniaProperty.Register<TimelineSlider, double>("MinLeftOffset");

    public Vector Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    public double Scale
    {
        get => GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
    
    protected override Type StyleKeyOverride => typeof(TimelineSlider);

    public double MinLeftOffset
    {
        get { return (double)GetValue(MinLeftOffsetProperty); }
        set { SetValue(MinLeftOffsetProperty, value); }
    }

    private Button _increaseButton;
    private Track _track;
    
    private bool _isDragging;
    private IDisposable? _increaseButtonSubscription;
    private IDisposable? _increaseButtonReleaseDispose;
    private IDisposable? _pointerMovedDispose;

    static TimelineSlider()
    {
        AffectsRender<TimelineSlider>(ScaleProperty, OffsetProperty, MinLeftOffsetProperty);
    }

    public TimelineSlider()
    {
        Maximum = int.MaxValue;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _increaseButtonSubscription?.Dispose();
        _increaseButtonReleaseDispose?.Dispose();
        _pointerMovedDispose?.Dispose();
        
        _increaseButton = e.NameScope.Find<Button>("PART_IncreaseButton");
        _track = e.NameScope.Find<Track>("PART_Track");
        
        if (_track != null)
        {
            _track.IgnoreThumbDrag = true;
        }

        if (_increaseButton != null)
        {
            _increaseButtonSubscription = _increaseButton.AddDisposableHandler(PointerPressedEvent, TrackPressed, RoutingStrategies.Tunnel);
            _increaseButtonReleaseDispose = _increaseButton.AddDisposableHandler(PointerReleasedEvent, TrackReleased, RoutingStrategies.Tunnel);
        }

        _pointerMovedDispose = this.AddDisposableHandler(PointerMovedEvent, TrackMoved, RoutingStrategies.Tunnel);
    }
    
    private void TrackPressed(object? sender, PointerPressedEventArgs e)
    {
        _isDragging = true;
        MoveToPoint(e.GetCurrentPoint(_track));
    }
    
    private void TrackReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDragging = false;
    }
    
    private void TrackMoved(object? sender, PointerEventArgs e)
    {
        if (!IsEnabled)
        {
            _isDragging = false;
            return;
        }

        if (_isDragging)
        {
            MoveToPoint(e.GetCurrentPoint(_track));
        }
    }
    
    private void MoveToPoint(PointerPoint point)
    {
        const double marginLeft = 15;
        
        double x = point.Position.X - marginLeft + Offset.X;
        int value = (int)Math.Round(x / Scale) + (int)Minimum;
        
        Value = value;
    }
}
