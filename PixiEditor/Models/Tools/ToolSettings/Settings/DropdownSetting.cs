using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System.Linq;
using Avalonia.Layout;

namespace PixiEditor.Models.Tools.ToolSettings.Settings
{
    public class DropdownSetting : Setting
    {
        public string[] Values { get; set; }

        public DropdownSetting(string name, string[] values, string label) : base(name)
        {
            Values = values;
            SettingControl = GenerateDropdown();
            Value = ((ComboBox)SettingControl).Items; //TODO: Fix later
            Label = label;
        }


        private ComboBox GenerateDropdown()
        {

            ComboBox combobox = new ComboBox
            {
                VerticalAlignment = VerticalAlignment.Center,
            };
            GenerateItems(combobox);
            return combobox;
        }

        private void GenerateItems(ComboBox comboBox)
        {
            for (int i = 0; i < Values.Length; i++)
            {
                ComboBoxItem item = new ComboBoxItem
                {
                    Content = Values[i]
                };
                comboBox.Items = comboBox.Items.Cast<ComboBoxItem>().Append(item);
            }
        }
    }
}