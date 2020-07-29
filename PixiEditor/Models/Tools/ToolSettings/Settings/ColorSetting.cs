using System.Windows.Data;
using System.Windows.Media;
using ColorPicker;

namespace PixiEditor.Models.Tools.ToolSettings.Settings
{
    public class ColorSetting : Setting
    {
        public ColorSetting(string name, string label = "") : base(name)
        {
            Label = label;
            SettingControl = GenerateColorPicker();
            Value = Color.FromArgb(0, 0, 0, 0);
        }

        private PortableColorPicker GenerateColorPicker()
        {
            PortableColorPicker picker = new PortableColorPicker();
            Binding binding = new Binding("Value")
            {
                Mode = BindingMode.TwoWay
            };
            picker.SetBinding(PortableColorPicker.SelectedColorProperty, binding);
            return picker;
        }
    }
}