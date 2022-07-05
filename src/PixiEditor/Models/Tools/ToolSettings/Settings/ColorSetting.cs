using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Media;
using PixiEditor.Helpers.Behaviours;
using PixiEditor.Views;
using PixiEditor.Views.UserControls;

namespace PixiEditor.Models.Tools.ToolSettings.Settings;

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
        resourceDictionary.Source = new System.Uri(
            "pack://application:,,,/ColorPicker;component/Styles/DefaultColorPickerStyle.xaml",
            System.UriKind.RelativeOrAbsolute);
        ToolSettingColorPicker picker = new ToolSettingColorPicker
        {
            Style = (Style)resourceDictionary["DefaultColorPickerStyle"]
        };

        Binding selectedColorBinding = new Binding("Value")
        {
            Mode = BindingMode.TwoWay
        };

        GlobalShortcutFocusBehavior behavior = new GlobalShortcutFocusBehavior();
        Interaction.GetBehaviors(picker).Add(behavior);
        picker.SetBinding(ToolSettingColorPicker.SelectedColorProperty, selectedColorBinding);
        return picker;
    }

    public override Control GenerateControl()
    {
        return GenerateColorPicker();
    }
}
