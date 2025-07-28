using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal sealed class BoolSettingViewModel : Setting<bool>
{
    public BoolSettingViewModel(string name, string label = "")
        : this(name, false, label)
    {
        AllowIconLabel = false;
    }

    public BoolSettingViewModel(string name, bool isChecked, string label = "")
        : base(name)
    {
        Label = label;
        Value = isChecked;
        IsLabelVisible = false;
    }
}
