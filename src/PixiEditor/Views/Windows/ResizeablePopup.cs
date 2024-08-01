using Avalonia;
using Avalonia.Controls;
using PixiEditor.Models.Dialogs;

namespace PixiEditor.Views.Windows;

internal class ResizeablePopup : Window
{
    public static readonly StyledProperty<int> NewPercentageSizeProperty =
        AvaloniaProperty.Register<ResizeablePopup, int>
            (nameof(NewPercentageSize), 0);

    public static readonly StyledProperty<SizeUnit> NewSelectedUnitProperty =
        AvaloniaProperty.Register<ResizeablePopup, SizeUnit>(
            nameof(NewSelectedUnit),
            SizeUnit.Pixel);

    public static readonly StyledProperty<int> NewAbsoluteHeightProperty =
        AvaloniaProperty<ResizeablePopup>.Register<ResizeablePopup, int>(
            nameof(NewAbsoluteHeight));

    public static readonly StyledProperty<int> NewAbsoluteWidthProperty =
        AvaloniaProperty.Register<ResizeablePopup, int>(
            nameof(NewAbsoluteWidth));

    public int NewPercentageSize
    {
        get => (int)GetValue(NewPercentageSizeProperty);
        set => SetValue(NewPercentageSizeProperty, value);
    }

    public SizeUnit NewSelectedUnit
    {
        get => (SizeUnit)GetValue(NewSelectedUnitProperty);
        set => SetValue(NewSelectedUnitProperty, value);
    }

    public int NewAbsoluteHeight
    {
        get => (int)GetValue(NewAbsoluteHeightProperty);
        set => SetValue(NewAbsoluteHeightProperty, value);
    }

    public int NewAbsoluteWidth
    {
        get => (int)GetValue(NewAbsoluteWidthProperty);
        set => SetValue(NewAbsoluteWidthProperty, value);
    }
}
