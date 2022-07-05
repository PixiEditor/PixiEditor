using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using PixiEditor.Views;
using PixiEditor.Views.UserControls;

namespace PixiEditor.Models.Tools.ToolSettings.Settings;

internal class SizeSetting : Setting<int>
{
    public SizeSetting(string name, string label = null)
        : base(name)
    {
        Value = 1;
        Label = label;
    }

    private SizeInput GenerateTextBox()
    {
        SizeInput tb = new SizeInput
        {
            Width = 65,
            Height = 20,
            VerticalAlignment = VerticalAlignment.Center,
            MaxSize = 9999,
            IsEnabled = true
        };

        Binding binding = new Binding("Value")
        {
            Mode = BindingMode.TwoWay,
        };
        tb.SetBinding(SizeInput.SizeProperty, binding);
        return tb;
    }

    public override Control GenerateControl()
    {
        return GenerateTextBox();
    }
}
