using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using PixiEditor.Views.Input;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal sealed class SizeSettingViewModel : Setting<int>
{
    private bool isEnabled = true;
    public SizeSettingViewModel(string name, string label = null)
        : base(name)
    {
        Label = label;
        Value = 1;
    }

    public bool IsEnabled
    {
        get => isEnabled;
        set
        {
            SetProperty(ref isEnabled, value);
        }
    }
}
