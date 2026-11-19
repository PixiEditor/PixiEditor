using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Interactivity;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.UI.Common.Converters;

namespace PixiEditor.Views.Dialogs;

public record SizePreset(string Name, int Width, int Height);

/// <summary>
///     Interaction logic for NewFilePopup.xaml.
/// </summary>
internal partial class NewFilePopup : PixiEditorPopup
{
    private bool _isUpdatingFromPreset = false;
    public static readonly StyledProperty<double> DpiProperty =
        AvaloniaProperty.Register<NewFilePopup, double>(nameof(Dpi), 300);

    public static readonly StyledProperty<MeasurementConverterData> SelectedMeasurementConverterDataProperty =
        AvaloniaProperty.Register<NewFilePopup, MeasurementConverterData>(nameof(SelectedMeasurementConverterData));

    public static readonly StyledProperty<MeasurementUnit> SelectedMeasurementUnitProperty =
        AvaloniaProperty.Register<NewFilePopup, MeasurementUnit>(nameof(SelectedMeasurementUnit), MeasurementUnit.Pixels);

    public static readonly StyledProperty<ObservableCollection<MeasurementUnit>> UnitsProperty =
        AvaloniaProperty.Register<NewFilePopup, ObservableCollection<MeasurementUnit>>(nameof(Units));

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

        SelectedMeasurementUnitProperty.Changed.Subscribe(MeasurementUnitChanged);
        DpiProperty.Changed.Subscribe(MeasurementUnitChanged);
    }

    public NewFilePopup()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += OnDialogShown;

        Units = new ObservableCollection<MeasurementUnit>(MeasurementUnit.AllKnown);
    }

    private void OnDialogShown(object sender, RoutedEventArgs e)
    {
        MinWidth = Width;
        sizePicker.FocusWidthPicker();
    }

    private static void MeasurementUnitChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e is { Sender: NewFilePopup { SelectedMeasurementUnit: not null } popup })
        {
            popup.SelectedMeasurementConverterData = new MeasurementConverterData(
                () => (float)popup.Dpi,
                (pixels, dpi) => popup.SelectedMeasurementUnit.FromPixels(pixels, dpi),
                (value, dpi) => popup.SelectedMeasurementUnit.ToPixels(value, dpi),
                () => popup.SelectedMeasurementUnit.Decimals);
        }
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

    public ObservableCollection<MeasurementUnit> Units
    {
        get { return (ObservableCollection<MeasurementUnit>)GetValue(UnitsProperty); }
        set { SetValue(UnitsProperty, value); }
    }

    public MeasurementUnit? SelectedMeasurementUnit
    {
        get { return (MeasurementUnit)GetValue(SelectedMeasurementUnitProperty); }
        set { SetValue(SelectedMeasurementUnitProperty, value); }
    }

    public MeasurementConverterData SelectedMeasurementConverterData
    {
        get { return (MeasurementConverterData)GetValue(SelectedMeasurementConverterDataProperty); }
        set { SetValue(SelectedMeasurementConverterDataProperty, value); }
    }

    public double Dpi
    {
        get { return (double)GetValue(DpiProperty); }
        set { SetValue(DpiProperty, value); }
    }
}
