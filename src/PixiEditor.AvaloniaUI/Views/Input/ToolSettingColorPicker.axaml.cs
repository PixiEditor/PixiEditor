using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace PixiEditor.AvaloniaUI.Views.Input;

internal partial class ToolSettingColorPicker : UserControl
{
    public static readonly StyledProperty<Color> SelectedColorProperty = AvaloniaProperty.Register<ToolSettingColorPicker, Color>(
        nameof(SelectedColor));

    public Color SelectedColor
    {
        get => GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }
    public ToolSettingColorPicker()
    {
        InitializeComponent();
        ColorPicker.SecondaryColor = Colors.Black;
    }
}

