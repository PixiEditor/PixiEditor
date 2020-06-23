using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace PixiEditor.Models.Tools.ToolSettings.Settings
{
    public class BoolSetting : Setting
    {
        public BoolSetting(string name, string label = "") : base(name)
        {
            Label = label;
            Value = false;
            SettingControl = GenerateCheckBox();
        }

        public BoolSetting(string name, bool isChecked, string label = "") : base(name)
        {
            Label = label;
            Value = isChecked;
            SettingControl = GenerateCheckBox();
        }

        private Control GenerateCheckBox()
        {
            CheckBox checkBox = new CheckBox
            {
                IsChecked = (bool) Value,
                VerticalAlignment = VerticalAlignment.Center
            };

            Binding binding = new Binding("Value")
            {
                Mode = BindingMode.TwoWay
            };

            checkBox.SetBinding(ToggleButton.IsCheckedProperty, binding);

            return checkBox;
        }
    }
}