using System.Windows;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Media;
using ColorPicker;
using PixiEditor.Helpers.Behaviours;
using PixiEditor.Views;

namespace PixiEditor.Models.Tools.ToolSettings.Settings
{
    public class ColorSetting : Setting<Color>
    {
        public ColorSetting(string name, string label = "")
            : base(name)
        {
            Label = label;
            SettingControl = GenerateColorPicker();
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

            Binding binding = new Binding("Value")
            {
                Mode = BindingMode.TwoWay
            };
            GlobalShortcutFocusBehavior behavor = new GlobalShortcutFocusBehavior();
            Interaction.GetBehaviors(picker).Add(behavor);
            picker.SetBinding(ToolSettingColorPicker.SelectedColorProperty, binding);
            return picker;
        }
    }
}