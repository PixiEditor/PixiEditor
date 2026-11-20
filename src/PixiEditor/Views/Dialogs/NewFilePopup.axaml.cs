using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Interactivity;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.Common;
using PixiEditor.Models.Config;
using PixiEditor.UI.Common.Converters;

namespace PixiEditor.Views.Dialogs;

public record SizePreset(string Name, double Width, double Height, string Unit, double Dpi);

public class SizePresets : List<SizePreset>, IMergeable<SizePresets>
{
    public SizePresets TryMergeWith(SizePresets other)
    {
        SizePresets presets = new SizePresets();
        presets.AddRange(other);
        presets.AddRange(this);

        return presets;
    }
}

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
        AvaloniaProperty.Register<NewFilePopup, MeasurementUnit>(nameof(SelectedMeasurementUnit),
            MeasurementUnit.Pixels);

    public static readonly StyledProperty<ObservableCollection<MeasurementUnit>> UnitsProperty =
        AvaloniaProperty.Register<NewFilePopup, ObservableCollection<MeasurementUnit>>(nameof(Units));

    public static readonly StyledProperty<int> FileHeightProperty =
        AvaloniaProperty.Register<NewFilePopup, int>(nameof(FileHeight));

    public static readonly StyledProperty<int> FileWidthProperty =
        AvaloniaProperty.Register<NewFilePopup, int>(nameof(FileWidth));

    public static readonly StyledProperty<SizePreset?> SelectedPresetProperty =
        AvaloniaProperty.Register<NewFilePopup, SizePreset?>(nameof(SelectedPreset));

    // Actual presets
    public ObservableCollection<SizePreset> Presets { get; } = new ObservableCollection<SizePreset>();

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
        SelectedMeasurementConverterData = new MeasurementConverterData(
            () => (float)Dpi,
            (pixels, dpi) =>
                Math.Round(SelectedMeasurementUnit.FromPixels(pixels, dpi), SelectedMeasurementUnit.Decimals),
            (value, dpi) => SelectedMeasurementUnit.ToPixels(value, dpi),
            () => SelectedMeasurementUnit.Decimals);
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
            popup.SelectedMeasurementConverterData.NotifyUpdated();
        }
    }

    private static void OnPresetChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is NewFilePopup popup && e.NewValue is SizePreset preset)
        {
            popup._isUpdatingFromPreset = true;

            var unit = MeasurementUnit.AllKnown.FirstOrDefault(u =>
                u.UnitName.Equals(preset.Unit, StringComparison.InvariantCultureIgnoreCase));
            if (unit != null)
            {
                popup.FileWidth = (int)unit.ToPixels(preset.Width, (float)preset.Dpi);
                popup.FileHeight = (int)unit.ToPixels(preset.Height, (float)preset.Dpi);
                popup.Dpi = preset.Dpi;
                popup.SelectedMeasurementUnit = unit;
            }

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
