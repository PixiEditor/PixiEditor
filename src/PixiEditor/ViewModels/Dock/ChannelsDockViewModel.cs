using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Models.DocumentModels;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.ViewModels.Dock;

internal class ChannelsDockViewModel : DockableViewModel
{
    public const string TabId = "ChannelsDock";

    public override string Id => TabId;
    public override string Title => new LocalizedString("CHANNELS_DOCK_TITLE");
    public override bool CanFloat => true;
    public override bool CanClose => true;

    public WindowViewModel WindowViewModel { get; }

    private ViewportWindowViewModel? _activeViewport;

    public ViewportWindowViewModel? ActiveViewport
    {
        get => _activeViewport;
        set => SetProperty(ref _activeViewport, value);
    }

    private ViewportColorChannels Channels
    {
        get => ActiveViewport?.Channels ?? ViewportColorChannels.Default;
        set
        {
            if (ActiveViewport != null)
            {
                ActiveViewport.Channels = value;
            }
        }
    }

    public ChannelsDockViewModel(WindowViewModel windowViewModel)
    {
        WindowViewModel = windowViewModel;
        windowViewModel.ActiveViewportChanged += WindowViewModelOnActiveViewportChanged;
    }

    private void WindowViewModelOnActiveViewportChanged(object? sender, ViewportWindowViewModel e)
    {
        if (ActiveViewport != null)
        {
            ActiveViewport.PropertyChanged -= ActiveViewportOnPropertyChanged;
        }

        ActiveViewport = e;
        ActiveViewport.PropertyChanged += ActiveViewportOnPropertyChanged;
    }

    private void ActiveViewportOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ViewportWindowViewModel.Channels))
        {
            return;
        }

        OnPropertyChanged(nameof(IsRedVisible));
        OnPropertyChanged(nameof(IsGreenVisible));
        OnPropertyChanged(nameof(IsBlueVisible));
        OnPropertyChanged(nameof(IsAlphaVisible));
        
        OnPropertyChanged(nameof(IsRedSolo));
        OnPropertyChanged(nameof(IsGreenSolo));
        OnPropertyChanged(nameof(IsBlueSolo));
        OnPropertyChanged(nameof(IsAlphaSolo));
    }

    public bool IsRedVisible
    {
        get => Channels.IsVisiblyVisible(ColorChannel.Red);
        set => SetVisible(ColorChannel.Red, value);
    }

    public bool IsRedSolo
    {
        get => Channels.IsSolo(ColorChannel.Red);
        set => SetSolo(ColorChannel.Red, value);
    }

    public bool IsGreenVisible
    {
        get => Channels.IsVisiblyVisible(ColorChannel.Green);
        set => SetVisible(ColorChannel.Green, value);
    }

    public bool IsGreenSolo
    {
        get => Channels.IsSolo(ColorChannel.Green);
        set => SetSolo(ColorChannel.Green, value);
    }

    public bool IsBlueVisible
    {
        get => Channels.IsVisiblyVisible(ColorChannel.Blue);
        set => SetVisible(ColorChannel.Blue, value);
    }

    public bool IsBlueSolo
    {
        get => Channels.IsSolo(ColorChannel.Blue);
        set => SetSolo(ColorChannel.Blue, value);
    }

    public bool IsAlphaVisible
    {
        get => Channels.IsVisiblyVisible(ColorChannel.Alpha);
        set => SetVisible(ColorChannel.Alpha, value);
    }

    public bool IsAlphaSolo
    {
        get => Channels.IsSolo(ColorChannel.Alpha);
        set => SetSolo(ColorChannel.Alpha, value);
    }

    private void SetVisible(ColorChannel channel, bool value)
    {
        var mode = Channels.GetModeForChannel(channel);

        if (mode.IsSolo && !value)
        {
            Channels = Channels.WithModeForChannel(channel, _ => new ColorChannelMode(), false);
        }
        else
        {
            Channels = Channels.WithModeForChannel(channel, x => x.WithVisible(value), value);
        }
    }

    private void SetSolo(ColorChannel channel, bool value)
    {
        var mode = Channels.GetModeForChannel(channel);

        Channels = Channels.WithModeForChannel(channel, x => x.WithSolo(value), value);
    }
}
