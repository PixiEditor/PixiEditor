using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace PixiEditor.Models.Tools.ToolSettings.Settings
{
    public class DropdownSetting : Setting<object>
    {
        public DropdownSetting(string name, string[] values, string label) : base(name)
        {
            Values = values;
            SettingControl = GenerateDropdown();
            Value = ((ComboBox) SettingControl).Items[0];
            Label = label;
        }

        public string[] Values { get; set; }


        private ComboBox GenerateDropdown()
        {
            var combobox = new ComboBox
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            GenerateItems(combobox);

            var binding = new Binding("Value")
            {
                Mode = BindingMode.TwoWay
            };
            combobox.SetBinding(Selector.SelectedValueProperty, binding);
            return combobox;
        }

        private void GenerateItems(ComboBox comboBox)
        {
            for (var i = 0; i < Values.Length; i++)
            {
                var item = new ComboBoxItem
                {
                    Content = Values[i]
                };
                comboBox.Items.Add(item);
            }
        }
    }
}