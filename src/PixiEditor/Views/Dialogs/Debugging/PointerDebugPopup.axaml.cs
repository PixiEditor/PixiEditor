using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Input;

namespace PixiEditor.Views.Dialogs.Debugging;

public partial class PointerDebugPopup : PixiEditorPopup
{
    private readonly Stopwatch pointerWatch = new();
    private readonly Stopwatch scrollWatch = new();
    
    public static readonly StyledProperty<PointerPointProperties> LastPointProperty =
        AvaloniaProperty.Register<PointerDebugPopup, PointerPointProperties>(nameof(LastPoint));

    public PointerPointProperties LastPoint
    {
        get => GetValue(LastPointProperty);
        set => SetValue(LastPointProperty, value);
    }
    
    public static readonly StyledProperty<PointerType> PointerTypeProperty =
        AvaloniaProperty.Register<PointerDebugPopup, PointerType>(nameof(PointerType));

    public PointerType PointerType
    {
        get => GetValue(PointerTypeProperty);
        set => SetValue(PointerTypeProperty, value);
    }
    
    public static readonly StyledProperty<PerformanceStats> PerformanceProperty =
        AvaloniaProperty.Register<PointerDebugPopup, PerformanceStats>(nameof(Performance));

    public PerformanceStats Performance
    {
        get => GetValue(PerformanceProperty);
        set => SetValue(PerformanceProperty, value);
    }

    public static readonly StyledProperty<bool> ShowDebugLineProperty =
        AvaloniaProperty.Register<PointerDebugPopup, bool>(nameof(ShowDebugLine), defaultValue: true);

    public bool ShowDebugLine
    {
        get => GetValue(ShowDebugLineProperty);
        set => SetValue(ShowDebugLineProperty, value);
    }

    public static readonly StyledProperty<ScrollStats> ScrollInfoProperty =
        AvaloniaProperty.Register<PointerDebugPopup, ScrollStats>(nameof(ScrollInfo));

    public ScrollStats ScrollInfo
    {
        get => GetValue(ScrollInfoProperty);
        set => SetValue(ScrollInfoProperty, value);
    }

    public PointerDebugPopup()
    {
        InitializeComponent();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        UpdateProperties(e);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        UpdateProperties(e);
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        DebugField.ClearPoints();
        Performance = default;
        ScrollInfo = default;
    }

    private void OnPointerWheel(object? sender, PointerWheelEventArgs e)
    {
        var current = ScrollInfo;
        var total = current.TotalScroll + e.Delta;

        var soonTm = new ScrollStats
        {
            TotalScroll = total,
            Delta = e.Delta,
            Minimum = new Vector(Math.Min(current.Minimum.X, e.Delta.X), Math.Min(current.Minimum.Y, e.Delta.Y)),
            Maximum = new Vector(Math.Max(current.Maximum.X, e.Delta.X), Math.Max(current.Maximum.Y, e.Delta.Y))
        };

        ScrollInfo = soonTm;

        var time = scrollWatch.Elapsed;
        scrollWatch.Restart();

        if (time.TotalMilliseconds != 0 && time.TotalMilliseconds < 1000)
        {
            var lastPerformance = Performance;
            double rate = 1000 / time.TotalMilliseconds;

            lastPerformance.LastScrollPoll = rate;
            lastPerformance.ScrollPolls++;
            lastPerformance.AverageScrollPoll = ((lastPerformance.ScrollPolls - 1) * lastPerformance.AverageScrollPoll + rate) / lastPerformance.ScrollPolls;

            Performance = lastPerformance;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var time = pointerWatch.Elapsed;
        pointerWatch.Restart();

        if (time.TotalMilliseconds != 0 && time.TotalMilliseconds < 250)
        {
            var lastPerformance = Performance;
            double rate = 1000 / time.TotalMilliseconds;

            lastPerformance.LastPollRate = rate;
            lastPerformance.CountedPolls++;
            lastPerformance.AveragePollRate = ((lastPerformance.CountedPolls - 1) * lastPerformance.AveragePollRate + rate) / lastPerformance.CountedPolls;

            Performance = lastPerformance;
        }

        var point = UpdateProperties(e);
        
        if (ShowDebugLine)
        {
            DebugField.ReportPoint(point);
        }
    }

    private PointerPoint UpdateProperties(PointerEventArgs e)
    {
        PointerType = e.Pointer.Type;
        var point = e.GetCurrentPoint(DebugField);
        LastPoint = point.Properties;

        return point;
    }

    public struct PerformanceStats
    {
        public double LastPollRate { get; set; }
        
        public int CountedPolls { get; set; }
        
        public double AveragePollRate { get; set; }
        
        public double LastScrollPoll { get; set; }
        
        public int ScrollPolls { get; set; }

        public double AverageScrollPoll { get; set; }
    }

    public struct ScrollStats
    {
        public Vector TotalScroll { get; set; }
        
        public Vector Delta { get; set; }
        
        public Vector Minimum { get; set; }
        
        public Vector Maximum { get; set; }
    }
}
