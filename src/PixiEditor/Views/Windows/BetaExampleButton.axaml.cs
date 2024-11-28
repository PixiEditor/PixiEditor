using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.AnalyticsAPI;
using PixiEditor.ViewModels;

namespace PixiEditor.Views.Windows;

public partial class BetaExampleButton : UserControl
{
    public static readonly StyledProperty<RelayCommand> CloseCommandProperty =
        AvaloniaProperty.Register<BetaExampleButton, RelayCommand>(nameof(CloseCommand));

    public static readonly StyledProperty<string> FileNameProperty =
        AvaloniaProperty.Register<BetaExampleButton, string>(nameof(FileName));

    public static readonly StyledProperty<string> DisplayNameProperty =
        AvaloniaProperty.Register<BetaExampleButton, string>(nameof(DisplayName));

    public static readonly StyledProperty<BetaExampleFile> BetaExampleFileProperty =
        AvaloniaProperty.Register<BetaExampleButton, BetaExampleFile>(nameof(BetaExampleFile));

    public RelayCommand CloseCommand
    {
        get => GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public string FileName
    {
        get => GetValue(FileNameProperty);
        set => SetValue(FileNameProperty, value);
    }

    public string DisplayName
    {
        get => GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }
    
    public BetaExampleFile BetaExampleFile
    {
        get => GetValue(BetaExampleFileProperty);
        set => SetValue(BetaExampleFileProperty, value);
    }

    public AsyncRelayCommand OpenCommand { get; }

    public BetaExampleButton()
    {
        OpenCommand = new AsyncRelayCommand(OpenExample);
        
        InitializeComponent();
        FileNameProperty.Changed.AddClassHandler((BetaExampleButton o, AvaloniaPropertyChangedEventArgs<string> args) => FileNameChanged(o, args));
    }

    private static void FileNameChanged(BetaExampleButton sender, AvaloniaPropertyChangedEventArgs<string> e)
    {
        sender.BetaExampleFile = new BetaExampleFile(e.NewValue.Value, sender.DisplayName);
    }
    
    private async Task OpenExample()
    {
        await using var stream = BetaExampleFile.GetStream();
        
        var bytes = new byte[stream.Length];
        await stream.ReadExactlyAsync(bytes);

        Application.Current.ForDesktopMainWindow(mainWindow => mainWindow.Activate());
        CloseCommand.Execute(null);
        
        ViewModelMain.Current.FileSubViewModel.OpenRecoveredDotPixi(null, bytes);
        Analytics.SendOpenExample(FileName);
    }

}
