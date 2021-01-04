using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ColorPicker;

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

        private PortableColorPicker GenerateColorPicker()
        {
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new System.Uri("pack://application:,,,/ColorPicker;component/Styles/DefaultColorPickerStyle.xaml",
                System.UriKind.RelativeOrAbsolute);
            PortableColorPicker picker = new PortableColorPicker
            {
                Style = (Style)resourceDictionary["DefaultColorPickerStyle"],
                SecondaryColor = System.Windows.Media.Colors.Black
            };
            Binding binding = new Binding("Value")
            {
                Mode = BindingMode.TwoWay
            };
            picker.SetBinding(PortableColorPicker.SelectedColorProperty, binding);
            return picker;
        }
    }
}