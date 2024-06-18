using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.Views.Animations;

[TemplatePart("PART_PlayToggle", typeof(ToggleButton))]
internal class Timeline : TemplatedControl
{
    public static readonly StyledProperty<KeyFrameCollection> KeyFramesProperty =
        AvaloniaProperty.Register<Timeline, KeyFrameCollection>(
            nameof(KeyFrames));

    public static readonly StyledProperty<int> ActiveFrameProperty =
        AvaloniaProperty.Register<Timeline, int>(nameof(ActiveFrame));

    public static readonly StyledProperty<bool> IsPlayingProperty = AvaloniaProperty.Register<Timeline, bool>(
        nameof(IsPlaying));

    public static readonly StyledProperty<ICommand> NewKeyFrameCommandProperty =
        AvaloniaProperty.Register<Timeline, ICommand>(
            nameof(NewKeyFrameCommand));

    public static readonly StyledProperty<double> ScaleProperty = AvaloniaProperty.Register<Timeline, double>(
        nameof(Scale), 100);
    
    public static readonly StyledProperty<int> FpsProperty = AvaloniaProperty.Register<Timeline, int>(nameof(Fps), 60);

    public static readonly StyledProperty<KeyFrameViewModel> SelectedKeyFrameProperty = AvaloniaProperty.Register<Timeline, KeyFrameViewModel>(
        "SelectedKeyFrame");

    public KeyFrameViewModel SelectedKeyFrame
    {
        get => GetValue(SelectedKeyFrameProperty);
        set => SetValue(SelectedKeyFrameProperty, value);
    }

    public double Scale
    {
        get => GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    public ICommand NewKeyFrameCommand
    {
        get => GetValue(NewKeyFrameCommandProperty);
        set => SetValue(NewKeyFrameCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> DuplicateKeyFrameCommandProperty =
        AvaloniaProperty.Register<Timeline, ICommand>(
            nameof(DuplicateKeyFrameCommand));

    public static readonly StyledProperty<ICommand> DeleteKeyFrameCommandProperty = AvaloniaProperty.Register<Timeline, ICommand>(
        nameof(DeleteKeyFrameCommand));

    public ICommand DeleteKeyFrameCommand
    {
        get => GetValue(DeleteKeyFrameCommandProperty);
        set => SetValue(DeleteKeyFrameCommandProperty, value);
    }
    
    public ICommand DuplicateKeyFrameCommand
    {
        get => GetValue(DuplicateKeyFrameCommandProperty);
        set => SetValue(DuplicateKeyFrameCommandProperty, value);
    }

    public bool IsPlaying
    {
        get => GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    public KeyFrameCollection KeyFrames
    {
        get => GetValue(KeyFramesProperty);
        set => SetValue(KeyFramesProperty, value);
    }

    public int ActiveFrame
    {
        get { return (int)GetValue(ActiveFrameProperty); }
        set { SetValue(ActiveFrameProperty, value); }
    }

    public int Fps
    {
        get { return (int)GetValue(FpsProperty); }
        set { SetValue(FpsProperty, value); }
    }

    public ICommand SelectKeyFrameCommand { get; }

    private ToggleButton? _playToggle;
    private DispatcherTimer _playTimer;

    static Timeline()
    {
        IsPlayingProperty.Changed.Subscribe(IsPlayingChanged);
        FpsProperty.Changed.Subscribe(FpsChanged);
        KeyFramesProperty.Changed.Subscribe(OnKeyFramesChanged);
    }

    public Timeline()
    {
        _playTimer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(1000f / Fps) };
        _playTimer.Tick += PlayTimerOnTick;
        SelectKeyFrameCommand = new RelayCommand<KeyFrameViewModel>(keyFrame =>
        {
            SelectedKeyFrame = keyFrame;
        });
    }

    public void Play()
    {
        IsPlaying = true;
    }

    public void Pause()
    {
        IsPlaying = false;
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
        if (ActiveFrame >= KeyFrames.FrameCount)
        {
            ActiveFrame = 0;
        }
        else
        {
            ActiveFrame++;
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

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        
        double newScale = Scale;
        
        int ticks = e.KeyModifiers.HasFlag(KeyModifiers.Control) ? 1 : 10;

        if (e.Delta.Y > 0)
        {
            newScale += ticks;
        }
        else
        {
            newScale -= ticks;
        }
        
        newScale = Math.Clamp(newScale, 1, 900);
        Scale = newScale;
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
    
    private static void FpsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is not Timeline timeline)
        {
            return;
        }

        timeline._playTimer.Interval = TimeSpan.FromMilliseconds(1000f / timeline.Fps);
    }
    
    private static void OnKeyFramesChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is not Timeline timeline)
        {
            return;
        }

        if(e.OldValue is KeyFrameCollection oldCollection)
        {
            oldCollection.KeyFrameAdded -= timeline.KeyFrames_KeyFrameAdded;
            oldCollection.KeyFrameRemoved -= timeline.KeyFrames_KeyFrameRemoved;
        }
        
        if(e.NewValue is KeyFrameCollection newCollection)
        {
            newCollection.KeyFrameAdded += timeline.KeyFrames_KeyFrameAdded;
            newCollection.KeyFrameRemoved += timeline.KeyFrames_KeyFrameRemoved;
        }
    }
    
    private void KeyFrames_KeyFrameAdded(KeyFrameViewModel keyFrame)
    {
        SelectedKeyFrame = keyFrame;
    }
    
    private void KeyFrames_KeyFrameRemoved(KeyFrameViewModel keyFrame)
    {
        if (SelectedKeyFrame == keyFrame)
        {
            SelectedKeyFrame = null;
        }
    }
}
