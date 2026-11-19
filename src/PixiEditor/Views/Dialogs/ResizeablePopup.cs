using Avalonia;
using PixiEditor.Models.Dialogs;
using PixiEditor.UI.Common.Converters;

namespace PixiEditor.Views.Dialogs;

internal class ResizeablePopup : PixiEditorPopup
{
    public static readonly StyledProperty<int> NewPercentageSizeProperty =
        AvaloniaProperty.Register<ResizeablePopup, int>(nameof(NewPercentageSize), 0);

    public static readonly StyledProperty<SizeUnit> NewSelectedUnitProperty =
        AvaloniaProperty.Register<ResizeablePopup, SizeUnit>(nameof(NewSelectedUnit), SizeUnit.Pixel);

    public static readonly StyledProperty<int> NewAbsoluteHeightProperty =
        AvaloniaProperty.Register<ResizeablePopup, int>(nameof(NewAbsoluteHeight), 0);

    public static readonly StyledProperty<int> NewAbsoluteWidthProperty =
        AvaloniaProperty.Register<ResizeablePopup, int>(nameof(NewAbsoluteWidth), 0);

    public static readonly StyledProperty<string> UnitProperty = AvaloniaProperty.Register<ResizeCanvasPopup, string>(
        nameof(Unit));

    public static readonly StyledProperty<MeasurementConverterData?> MeasurementConverterProperty = AvaloniaProperty.Register<ResizeCanvasPopup, MeasurementConverterData?>(
        nameof(MeasurementConverter));

    public MeasurementConverterData? MeasurementConverter
    {
        get => GetValue(MeasurementConverterProperty);
        set => SetValue(MeasurementConverterProperty, value);
    }

    public string Unit
    {
        get => GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public int NewPercentageSize
    {
        get => GetValue(NewPercentageSizeProperty);
        set => SetValue(NewPercentageSizeProperty, value);
    }

    public SizeUnit NewSelectedUnit
    {
        get => GetValue(NewSelectedUnitProperty);
        set => SetValue(NewSelectedUnitProperty, value);
    }

    public int NewAbsoluteHeight
    {
        get => GetValue(NewAbsoluteHeightProperty);
        set => SetValue(NewAbsoluteHeightProperty, value);
    }

    public int NewAbsoluteWidth
    {
        get => GetValue(NewAbsoluteWidthProperty);
        set => SetValue(NewAbsoluteWidthProperty, value);
    }
}
