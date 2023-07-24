using System.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

internal sealed class BoolSetting : Setting<bool>
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

        checkBox.Bind(ToggleButton.IsCheckedProperty, binding);

        return checkBox;
    }

    public override Control GenerateControl()
    {
        return GenerateCheckBox();
    }
}
