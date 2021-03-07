using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace PixiEditor.Models.Tools.ToolSettings.Settings
{
    public class EnumSetting<TEnum> : Setting<TEnum, ComboBox>
        where TEnum : struct, Enum
    {
        private int selectedIndex = 0;

        /// <summary>
        /// Gets or sets the selected Index of the <see cref="ComboBox"/>.
        /// </summary>
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (SetProperty(ref selectedIndex, value))
                {
                    RaisePropertyChanged(nameof(Value));
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected value of the <see cref="ComboBox"/>.
        /// </summary>
        public new TEnum Value
        {
            get => (TEnum)(SettingControl.SelectedItem as ComboBoxItem).Tag;
            set
            {
                SettingControl.SelectedItem = SettingControl.Items.Cast<ComboBoxItem>().First(x => x.Tag == (object)value);
                RaisePropertyChanged(nameof(Value));
            }
        }

        public EnumSetting(string name, string label)
            : base(name)
        {
            SettingControl = GenerateDropdown();

            Label = label;
        }

        public EnumSetting(string name, string label, TEnum defaultValue)
            : this(name, label)
        {
            Value = defaultValue;
        }

        private static ComboBox GenerateDropdown()
        {
            ComboBox combobox = new ComboBox
            {
                VerticalAlignment = VerticalAlignment.Center
            };

            GenerateItems(combobox);

            Binding binding = new Binding(nameof(SelectedIndex))
            {
                Mode = BindingMode.TwoWay
            };

            combobox.SetBinding(Selector.SelectedIndexProperty, binding);

            return combobox;
        }

        private static void GenerateItems(ComboBox comboBox)
        {
            string[] names = Enum.GetNames<TEnum>();
            TEnum[] values = Enum.GetValues<TEnum>();

            for (int i = 0; i < names.Length; i++)
            {
                ComboBoxItem item = new ComboBoxItem
                {
                    Content = names[i],
                    Tag = values[i]
                };

                comboBox.Items.Add(item);
            }
        }
    }
}