using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Localization;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

internal sealed class EnumSetting<TEnum> : Setting<TEnum, ComboBox>
    where TEnum : struct, Enum
{
    private int selectedIndex;

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
    public override TEnum Value
    {
        get => Enum.GetValues<TEnum>()[SelectedIndex];
        set
        {
            var values = Enum.GetValues<TEnum>();

            for (var i = 0; i < values.Length; i++)
            {
                if (values[i].Equals(value))
                {
                    SelectedIndex = i;
                    break;
                }
            }

            base.Value = value;
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
        var combobox = new ComboBox
        {
            VerticalAlignment = VerticalAlignment.Center
        };

        GenerateItems(combobox);

        var binding = new Binding(nameof(SelectedIndex))
        {
            Mode = BindingMode.TwoWay
        };

        combobox.SetBinding(Selector.SelectedIndexProperty, binding);

        return combobox;
    }

    private static void GenerateItems(ComboBox comboBox)
    {
        var values = Enum.GetValues<TEnum>();

        foreach (var value in values)
        {
            var item = new ComboBoxItem
            {
                Content = value.GetDescription(),
                Tag = value
            };

            comboBox.Items.Add(item);
        }
    }
}
