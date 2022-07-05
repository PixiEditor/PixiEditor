using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PixiEditor.Views;

/// <summary>
/// Interaction logic for ToolSettingColorPicker.xaml.
/// </summary>
internal partial class ToolSettingColorPicker : UserControl
{
    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ToolSettingColorPicker));

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set { SetValue(SelectedColorProperty, value); }
    }

    public ToolSettingColorPicker()
    {
        InitializeComponent();
        ColorPicker.SecondaryColor = Colors.Black;
    }
}
