using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using PixiEditor.Helpers.Extensions;

namespace PixiEditor.Models.Tools.ToolSettings.Settings;

internal class EnumSetting<TEnum> : Setting<TEnum, ComboBox>
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
            for (int i = 0; i < SettingControl.Items.Count; i++)
            {
                ComboBoxItem item = SettingControl.Items[i] as ComboBoxItem;

                if (item.Tag.Equals(value))
                {
                    SelectedIndex = i;
                }
            }
        }
    }

    public override Control GenerateControl()
    {
        return GenerateDropdown();
    }

    public EnumSetting(string name, string label)
        : base(name)
    {
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
        TEnum[] values = Enum.GetValues<TEnum>();

        foreach (TEnum value in values)
        {
            ComboBoxItem item = new ComboBoxItem
            {
                Content = value.GetDescription(),
                Tag = value
            };

            comboBox.Items.Add(item);
        }
    }
}
