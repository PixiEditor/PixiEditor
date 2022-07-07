using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

internal class BoolSetting : Setting<bool>
{
    public BoolSetting(string name, string label = "")
        : this(name, false, label)
    {
    }

    public BoolSetting(string name, bool isChecked, string label = "")
        : base(name)
    {
        Label = label;
        Value = isChecked;
    }

    private Control GenerateCheckBox()
    {
        var checkBox = new CheckBox
        {
            IsChecked = Value,
            VerticalAlignment = VerticalAlignment.Center
        };

        var binding = new Binding("Value")
        {
            Mode = BindingMode.TwoWay
        };

        checkBox.SetBinding(ToggleButton.IsCheckedProperty, binding);

        return checkBox;
    }

    public override Control GenerateControl()
    {
        return GenerateCheckBox();
    }
}
