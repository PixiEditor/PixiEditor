using Avalonia.Media;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal sealed class ColorSettingViewModel : Setting<Color>
{
    public ColorSettingViewModel(string name, string label = "") : this(name, Colors.White, label)
    { }
    
    public ColorSettingViewModel(string name, Color defaultValue, string label = "")
        : base(name)
    {
        Label = label;
        Value = defaultValue;
    }
}
