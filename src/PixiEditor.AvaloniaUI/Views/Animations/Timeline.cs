using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.Views.Animations;

[TemplatePart("PART_PlayToggle", typeof(ToggleButton))]
public class Timeline : TemplatedControl
{
    public static readonly StyledProperty<ObservableCollection<IKeyFrameHandler>> KeyFramesProperty =
        AvaloniaProperty.Register<Timeline, ObservableCollection<IKeyFrameHandler>>(
            nameof(KeyFrames));

    public static readonly StyledProperty<int> ActiveFrameProperty =
        AvaloniaProperty.Register<Timeline, int>(nameof(ActiveFrame));

    public static readonly StyledProperty<bool> IsPlayingProperty = AvaloniaProperty.Register<Timeline, bool>(
        nameof(IsPlaying));

    public bool IsPlaying
    {
        get => GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    public ObservableCollection<IKeyFrameHandler> KeyFrames
    {
        get => GetValue(KeyFramesProperty);
        set => SetValue(KeyFramesProperty, value);
    }

    public int ActiveFrame
    {
        get { return (int)GetValue(ActiveFrameProperty); }
        set { SetValue(ActiveFrameProperty, value); }
    }

    private ToggleButton? _playToggle;
    private DispatcherTimer _playTimer;

    static Timeline()
    {
        IsPlayingProperty.Changed.Subscribe(IsPlayingChanged);
    }

    public Timeline()
    {
        _playTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000 / 60f) };
        _playTimer.Tick += PlayTimerOnTick;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _playToggle = e.NameScope.Find<ToggleButton>("PART_PlayToggle");
        
        if (_playToggle != null)
        {
            _playToggle.Click += PlayToggleOnClick;
        }
    }
    
    private void PlayTimerOnTick(object? sender, EventArgs e)
    {
        ActiveFrame++;
        
        if (ActiveFrame >= KeyFrames.Count)
        {
            ActiveFrame = 0;
        }
    }

    private void PlayToggleOnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton toggleButton)
        {
            return;
        }

        if (toggleButton.IsChecked == true)
        {
            IsPlaying = true;
        }
        else
        {
            IsPlaying = false;
        }
    }
    
    public void Play()
    {
        IsPlaying = true;
    }
    
    public void Pause()
    {
        IsPlaying = false;
    }
    
    private static void IsPlayingChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is not Timeline timeline)
        {
            return;
        }

        if (timeline.IsPlaying)
        {
            timeline._playTimer.Start();
        }
        else
        {
            timeline._playTimer.Stop();
        }
    }
}
