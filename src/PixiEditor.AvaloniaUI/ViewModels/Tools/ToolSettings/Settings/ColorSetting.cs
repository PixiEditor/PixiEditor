using Avalonia.Controls;
using Avalonia.Media;

namespace PixiEditor.AvaloniaUI.ViewModels.Tools.ToolSettings.Settings;

internal sealed class ColorSetting : Setting<Color>
{
    public ColorSetting(string name, string label = "") : this(name, Colors.White, label)
    { }
    
    public ColorSetting(string name, Color defaultValue, string label = "")
        : base(name)
    {
        Label = label;
        Value = defaultValue;
    }

    //TODO: Implement
    /*private ToolSettingColorPicker GenerateColorPicker()
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
    }*/

    public override Control GenerateControl()
    {
        return new Border();
        //return GenerateColorPicker();
    }
}
