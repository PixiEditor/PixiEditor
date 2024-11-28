using Avalonia;
using Avalonia.Styling;
using PixiEditor.Models.Dialogs;

namespace PixiEditor.Views.Dialogs;

internal class ResizeablePopup : PixiEditorPopup, IStyleable
{
    public static readonly StyledProperty<int> NewPercentageSizeProperty =
        AvaloniaProperty.Register<ResizeablePopup, int>(nameof(NewPercentageSize), 0);

    public static readonly StyledProperty<SizeUnit> NewSelectedUnitProperty =
        AvaloniaProperty.Register<ResizeablePopup, SizeUnit>(nameof(NewSelectedUnit), SizeUnit.Pixel);

    public static readonly StyledProperty<int> NewAbsoluteHeightProperty =
        AvaloniaProperty.Register<ResizeablePopup, int>(nameof(NewAbsoluteHeight), 0);

    public static readonly StyledProperty<int> NewAbsoluteWidthProperty =
        AvaloniaProperty.Register<ResizeablePopup, int>(nameof(NewAbsoluteWidth), 0);

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
