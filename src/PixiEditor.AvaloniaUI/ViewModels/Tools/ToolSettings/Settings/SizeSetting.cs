using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using PixiEditor.AvaloniaUI.Views.Input;

namespace PixiEditor.AvaloniaUI.ViewModels.Tools.ToolSettings.Settings;

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
            VerticalAlignment = VerticalAlignment.Center,
            MaxSize = 9999,
            IsEnabled = true
        };

        Binding binding = new Binding("Value")
        {
            Mode = BindingMode.TwoWay,
        };
        tb.Bind(SizeInput.SizeProperty, binding);
        return tb;
    }

    public override Control GenerateControl()
    {
        return GenerateTextBox();
    }
}
