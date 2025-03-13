using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PixiEditor.Helpers.Behaviours;

namespace PixiEditor.Views.Input;

internal partial class ToolSettingColorPicker : UserControl
{
    public static readonly StyledProperty<IBrush> SelectedBrushProperty =
        AvaloniaProperty.Register<ToolSettingColorPicker, IBrush>(
            nameof(SelectedBrush));

    public IBrush SelectedBrush
    {
        get => GetValue(SelectedBrushProperty);
        set => SetValue(SelectedBrushProperty, value);
    }

    public ToolSettingColorPicker()
    {
        InitializeComponent();
        ColorPicker.SelectedColor = Colors.White;
        ColorPicker.SecondaryColor = Colors.Black;
        ColorPicker.TemplateApplied += ColorPickerOnTemplateApplied;
    }

    private void ColorPickerOnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        ColorPicker.FindDescendantOfType<ToggleButton>().Focusable = false;
    }
}
