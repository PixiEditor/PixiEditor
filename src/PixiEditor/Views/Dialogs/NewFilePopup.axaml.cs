using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Interactivity;

namespace PixiEditor.Views.Dialogs;

/// <summary>
///     Interaction logic for NewFilePopup.xaml.
/// </summary>
internal partial class NewFilePopup : PixiEditorPopup
{
    public record SizePreset(string Name, int Width, int Height);

    private bool _isUpdatingFromPreset = false;

    public static readonly StyledProperty<int> FileHeightProperty =
        AvaloniaProperty.Register<NewFilePopup, int>(nameof(FileHeight));

    public static readonly StyledProperty<int> FileWidthProperty =
        AvaloniaProperty.Register<NewFilePopup, int>(nameof(FileWidth));

    public static readonly StyledProperty<SizePreset?> SelectedPresetProperty =
        AvaloniaProperty.Register<NewFilePopup, SizePreset?>(nameof(SelectedPreset));

    // Actual presets
    public List<SizePreset> Presets { get; } = new()
    {
        new("32x32", 32, 32),
        new("64x64", 64, 64),
        new("128x128", 128, 128),
        new("256x256", 256, 256),
        new("512x512", 512, 512),
        new("1024x1024", 1024, 1024),
        new("1280x720 (HD)", 1280, 720),
        new("1920x1080 (FHD)", 1920, 1080),
        new("2560x1440 (QHD)", 2560, 1440)
    };

    static NewFilePopup()
    {
        SelectedPresetProperty.Changed.Subscribe(OnPresetChanged);
        
        FileWidthProperty.Changed.Subscribe(OnDimensionsChanged);
        FileHeightProperty.Changed.Subscribe(OnDimensionsChanged);
    }

    public NewFilePopup()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += OnDialogShown;
    }

    private void OnDialogShown(object sender, RoutedEventArgs e)
    {
        MinWidth = Width;
        sizePicker.FocusWidthPicker();
    }

    private static void OnPresetChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is NewFilePopup popup && e.NewValue is SizePreset preset)
        {
            popup._isUpdatingFromPreset = true;

            popup.FileWidth = preset.Width;
            popup.FileHeight = preset.Height;

            popup._isUpdatingFromPreset = false;
        }
    }

    private static void OnDimensionsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is NewFilePopup popup)
        {
            if (popup._isUpdatingFromPreset) return;

            if (popup.SelectedPreset != null)
            {
                popup.SelectedPreset = null;
            }
        }
    }

    public int FileHeight
    {
        get => (int)GetValue(FileHeightProperty);
        set => SetValue(FileHeightProperty, value);
    }

    public int FileWidth
    {
        get => (int)GetValue(FileWidthProperty);
        set => SetValue(FileWidthProperty, value);
    }

    public SizePreset? SelectedPreset
    {
        get => GetValue(SelectedPresetProperty);
        set => SetValue(SelectedPresetProperty, value);
    }
}