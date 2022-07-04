using PixiEditor.Models.Enums;
using System.Windows;

namespace PixiEditor.Views;

public class ResizeablePopup : Window
{
    public static readonly DependencyProperty NewPercentageSizeProperty =
        DependencyProperty.Register(nameof(NewPercentageSize), typeof(int), typeof(ResizeablePopup), new PropertyMetadata(0));

    public static readonly DependencyProperty NewSelectedUnitProperty =
        DependencyProperty.Register(nameof(NewSelectedUnit), typeof(SizeUnit), typeof(SizePicker), new PropertyMetadata(SizeUnit.Pixel));

    public static readonly DependencyProperty NewAbsoluteHeightProperty =
        DependencyProperty.Register(nameof(NewAbsoluteHeight), typeof(int), typeof(ResizeablePopup), new PropertyMetadata(0));

    public static readonly DependencyProperty NewAbsoluteWidthProperty =
        DependencyProperty.Register(nameof(NewAbsoluteWidth), typeof(int), typeof(ResizeablePopup), new PropertyMetadata(0));

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