using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PixiEditor.Views.Main;

public partial class MiniAnimationPlayer : UserControl
{
    public static readonly StyledProperty<int> ActiveFrameProperty = AvaloniaProperty.Register<MiniAnimationPlayer, int>("ActiveFrame");

    public static readonly StyledProperty<int> FramesCountProperty =
        AvaloniaProperty.Register<MiniAnimationPlayer, int>("FramesCount");

    public static readonly StyledProperty<bool> IsPlayingProperty = AvaloniaProperty.Register<MiniAnimationPlayer, bool>(
        nameof(IsPlaying));

    public bool IsPlaying
    {
        get => GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    public int FramesCount
    {
        get { return (int)GetValue(FramesCountProperty); }
        set { SetValue(FramesCountProperty, value); }
    }

    public int ActiveFrame
    {
        get { return (int)GetValue(ActiveFrameProperty); }
        set { SetValue(ActiveFrameProperty, value); }
    }

    public MiniAnimationPlayer()
    {
        InitializeComponent();
    }
}
