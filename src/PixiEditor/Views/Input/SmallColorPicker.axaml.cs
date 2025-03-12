using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ColorPicker;
using ColorPicker.Models;

namespace PixiEditor.Views.Input;

internal partial class SmallColorPicker : DualPickerControlBase, IGradientStorage
{
    public static readonly StyledProperty<GradientState> GradientStateProperty = AvaloniaProperty.Register<SmallColorPicker, GradientState>(
        nameof(GradientState));

    public static readonly StyledProperty<bool> EnableGradientsTabProperty = AvaloniaProperty.Register<SmallColorPicker, bool>(
        nameof(EnableGradientsTab));

    public static readonly StyledProperty<bool> EnableRecentBrushesProperty = AvaloniaProperty.Register<SmallColorPicker, bool>(
        nameof(EnableRecentBrushes), true);

    public bool EnableRecentBrushes
    {
        get => GetValue(EnableRecentBrushesProperty);
        set => SetValue(EnableRecentBrushesProperty, value);
    }

    public bool EnableGradientsTab
    {
        get => GetValue(EnableGradientsTabProperty);
        set => SetValue(EnableGradientsTabProperty, value);
    }

    public GradientState GradientState
    {
        get => GetValue(GradientStateProperty);
        set => SetValue(GradientStateProperty, value);
    }

    public static readonly StyledProperty<IBrush> SelectedBrushProperty = AvaloniaProperty.Register<SmallColorPicker, IBrush>(
        nameof(SelectedBrush));

    public IBrush SelectedBrush
    {
        get => GetValue(SelectedBrushProperty);
        set => SetValue(SelectedBrushProperty, value);
    }

    public SmallColorPicker()
    {
        InitializeComponent();
    }
}

