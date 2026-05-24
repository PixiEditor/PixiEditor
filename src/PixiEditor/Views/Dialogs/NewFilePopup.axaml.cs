using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using PixiEditor.UI.Common.Localization;
using PixiEditor.Views.Input;

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

    public static readonly StyledProperty<bool> UseCustomBackgroundProperty =
        AvaloniaProperty.Register<NewFilePopup, bool>(nameof(UseCustomBackground));

    public static readonly StyledProperty<IBrush> BackgroundBrushProperty =
        AvaloniaProperty.Register<NewFilePopup, IBrush>(
            nameof(BackgroundBrush),
            defaultValue: Brushes.White,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

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
        BackgroundBrush = Brushes.White;
    }

    private Window? _bgColorWindow;

    private async void BgSwatch_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_bgColorWindow is { IsVisible: true })
        {
            _bgColorWindow.Activate();
            return;
        }

        IBrush originalBrush = BackgroundBrush ?? Brushes.White;

        var picker = new SmallColorPicker
        {
            Width = 240,
            Height = 380,
            EnableGradientsTab = false,
            SelectedBrush = originalBrush
        };

        var subscription = picker.GetObservable(SmallColorPicker.SelectedBrushProperty).Subscribe(brush =>
        {
            if (brush is not null)
            {
                BackgroundBrush = brush;
            }
        });

        var okButton = new Button
        {
            IsDefault = true,
            MinWidth = 70,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Translator.SetKey(okButton, "OK");

        var cancelButton = new Button
        {
            IsCancel = true,
            MinWidth = 70,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Translator.SetKey(cancelButton, "CANCEL");

        bool confirmed = false;
        okButton.Click += (_, _) =>
        {
            confirmed = true;
            _bgColorWindow?.Close();
        };
        cancelButton.Click += (_, _) =>
        {
            confirmed = false;
            _bgColorWindow?.Close();
        };

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0)
        };
        buttonRow.Children.Add(cancelButton);
        buttonRow.Children.Add(okButton);

        var layout = new DockPanel { LastChildFill = true };
        DockPanel.SetDock(buttonRow, global::Avalonia.Controls.Dock.Bottom);
        layout.Children.Add(buttonRow);
        layout.Children.Add(picker);

        _bgColorWindow = new Window
        {
            Width = 260,
            Height = 460,
            CanResize = false,
            ShowInTaskbar = false,
            SystemDecorations = SystemDecorations.Full,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = this.Background,
            Content = new Border
            {
                Padding = new Thickness(10),
                Child = layout
            }
        };
        Translator.SetKey(_bgColorWindow, "BACKGROUND_COLOR");

        _bgColorWindow.Closed += (_, _) =>
        {
            subscription.Dispose();
            if (!confirmed)
            {
                BackgroundBrush = originalBrush;
            }
            _bgColorWindow = null;
        };

        await _bgColorWindow.ShowDialog(this);
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

    public bool UseCustomBackground
    {
        get => GetValue(UseCustomBackgroundProperty);
        set => SetValue(UseCustomBackgroundProperty, value);
    }

    public IBrush BackgroundBrush
    {
        get => GetValue(BackgroundBrushProperty);
        set => SetValue(BackgroundBrushProperty, value);
    }
}
