using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Drawie.Backend.Core;
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

    private static Dictionary<string, BetaExampleFile> exampleFilesCache = new();

    public BetaExampleButton()
    {
        OpenCommand = new AsyncRelayCommand(OpenExample);

        InitializeComponent();
        FileNameProperty.Changed.AddClassHandler((BetaExampleButton o, AvaloniaPropertyChangedEventArgs<string> args) =>
            FileNameChanged(o, args));
    }

    private static void FileNameChanged(BetaExampleButton sender, AvaloniaPropertyChangedEventArgs<string> e)
    {
        if (e.OldValue.Value != null)
        {
            if (exampleFilesCache.ContainsKey(e.OldValue.Value))
            {
                var oldFile = exampleFilesCache[e.OldValue.Value];
                oldFile.Dispose();

                exampleFilesCache.Remove(e.OldValue.Value);
            }
        }

        if (e.NewValue.HasValue == false || string.IsNullOrWhiteSpace(e.NewValue.Value))
        {
            return;
        }

        if (exampleFilesCache.ContainsKey(e.NewValue.Value))
        {
            sender.BetaExampleFile = exampleFilesCache[e.NewValue.Value];
            return;
        }

        sender.BetaExampleFile = new BetaExampleFile(e.NewValue.Value, sender.DisplayName);
        exampleFilesCache.Add(e.NewValue.Value, sender.BetaExampleFile);
    }

    private async Task OpenExample()
    {
        await using var stream = BetaExampleFile.GetStream();

        var bytes = new byte[stream.Length];
        await stream.ReadExactlyAsync(bytes);

        Application.Current.ForDesktopMainWindow(mainWindow => mainWindow.Activate());
        CloseCommand.Execute(null);

        ViewModelMain.Current.FileSubViewModel.OpenFromPixiBytes(bytes);
        ViewModelMain.Current.DocumentManagerSubViewModel.Documents[^1].Operations.UseSrgbProcessing();
        ViewModelMain.Current.DocumentManagerSubViewModel.Documents[^1].Operations.ClearUndo();
        Analytics.SendOpenExample(FileName);
    }
}
