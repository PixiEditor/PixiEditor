using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Xaml.Interactivity;
using PixiEditor.Helpers.Behaviours;
using PixiEditor.Views.Input;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

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
        var picker = new ToolSettingColorPicker();

        var selectedColorBinding = new Binding("Value")
        {
            Mode = BindingMode.TwoWay
        };

        var behavior = new GlobalShortcutFocusBehavior();
        Interaction.GetBehaviors(picker).Add(behavior);
        picker.Bind(ToolSettingColorPicker.SelectedColorProperty, selectedColorBinding);
        return picker;
    }

    public override Control GenerateControl()
    {
        return GenerateColorPicker();
    }
}
