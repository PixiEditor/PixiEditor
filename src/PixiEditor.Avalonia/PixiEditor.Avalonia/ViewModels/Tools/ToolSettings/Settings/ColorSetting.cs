using System.Windows.Media;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;
using PixiEditor.Views.UserControls;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

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
