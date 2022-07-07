using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Media;
using PixiEditor.Helpers.Behaviours;
using PixiEditor.Views.UserControls;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

internal class ColorSetting : Setting<Color>
{
    public ColorSetting(string name, string label = "")
        : base(name)
    {
        Label = label;
        Value = Color.FromArgb(255, 255, 255, 255);
    }

    private ToolSettingColorPicker GenerateColorPicker()
    {
        var resourceDictionary = new ResourceDictionary();
        resourceDictionary.Source = new Uri(
            "pack://application:,,,/ColorPicker;component/Styles/DefaultColorPickerStyle.xaml",
            UriKind.RelativeOrAbsolute);
        var picker = new ToolSettingColorPicker
        {
            Style = (Style)resourceDictionary["DefaultColorPickerStyle"]
        };

        var selectedColorBinding = new Binding("Value")
        {
            Mode = BindingMode.TwoWay
        };

        var behavior = new GlobalShortcutFocusBehavior();
        Interaction.GetBehaviors(picker).Add(behavior);
        picker.SetBinding(ToolSettingColorPicker.SelectedColorProperty, selectedColorBinding);
        return picker;
    }

    public override Control GenerateControl()
    {
        return GenerateColorPicker();
    }
}
