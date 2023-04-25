using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using PixiEditor.Views.UserControls;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

internal sealed class SizeSetting : Setting<int>
{
    public SizeSetting(string name, string label = null)
        : base(name)
    {
        Label = label;
        Value = 1;
    }

    private SizeInput GenerateTextBox()
    {
        SizeInput tb = new SizeInput
        {
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
