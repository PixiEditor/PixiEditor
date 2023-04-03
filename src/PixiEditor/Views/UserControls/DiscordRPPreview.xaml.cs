using System.Windows;
using System.Windows.Controls;
using PixiEditor.Localization;

namespace PixiEditor.Views.UserControls;

/// <summary>
/// Interaction logic for DiscordRPPreview.xaml
/// </summary>
internal partial class DiscordRPPreview : UserControl
{
    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(nameof(State), typeof(string), typeof(DiscordRPPreview), new PropertyMetadata(new LocalizedString("DISCORD_STATE").Value));

    public string State
    {
        get => (string)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly DependencyProperty DetailProperty =
        DependencyProperty.Register(nameof(Detail), typeof(string), typeof(DiscordRPPreview), new PropertyMetadata(new LocalizedString("DISCORD_DETAILS").Value));

    public string Detail
    {
        get => (string)GetValue(DetailProperty);
        set => SetValue(DetailProperty, value);
    }

    public static readonly DependencyProperty UserSourceProperty =
        DependencyProperty.Register(nameof(UserSource), typeof(string), typeof(DiscordRPPreview), new PropertyMetadata("../../Images/pixiBotLogo.png"));

    public string UserSource
    {
        get => (string)GetValue(UserSourceProperty);
        set => SetValue(UserSourceProperty, value);
    }

    public static readonly DependencyProperty IsPlayingProperty =
        DependencyProperty.Register(nameof(IsPlaying), typeof(bool), typeof(DiscordRPPreview), new PropertyMetadata(true));

    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty);
        set
        {
            SetValue(IsPlayingProperty, value);
        }
    }

    public DiscordRPPreview()
    {
        InitializeComponent();
    }
}
