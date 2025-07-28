using Avalonia;
using Avalonia.Controls;

namespace PixiEditor.Views.Windows.Settings;

public partial class DiscordRichPresencePreview : UserControl
{
    public DiscordRichPresencePreview()
    {
        InitializeComponent();
    }
    
    public static readonly StyledProperty<string> StateProperty =
        AvaloniaProperty.Register<DiscordRichPresencePreview, string>(nameof(State), "16x16, 2 Layers");

    public string State
    {
        get => (string)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly StyledProperty<string> DetailProperty =
        AvaloniaProperty.Register<DiscordRichPresencePreview, string>(nameof(Detail), "Editing an image");

    public string Detail
    {
        get => (string)GetValue(DetailProperty);
        set => SetValue(DetailProperty, value);
    }

    public static readonly StyledProperty<string> UserSourceProperty =
        AvaloniaProperty.Register<DiscordRichPresencePreview, string>(nameof(UserSource), "/Images/PixiBotLogo.png");

    public string UserSource
    {
        get => (string)GetValue(UserSourceProperty);
        set => SetValue(UserSourceProperty, value);
    }

    public static readonly StyledProperty<bool> IsPlayingProperty =
        AvaloniaProperty.Register<DiscordRichPresencePreview, bool>(nameof(IsPlaying), true);

    public bool IsPlaying
    {
        get => GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }
}

