using System.Windows.Data;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

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

        private ColorPicker GenerateColorPicker()
        {
            ColorPicker picker = new ColorPicker()
            {
                UsingAlphaChannel = true,
                AvailableColorsSortingMode = ColorSortingMode.Alphabetical,
                Width = 70
            };
            Binding binding = new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
            };
            picker.SetBinding(ColorPicker.SelectedColorProperty, binding);
            return picker;
        }
    }
}
