namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal class StringSettingViewModel : Setting<string>
{
    public StringSettingViewModel(string name, string label) : base(name)
    {
        Label = label;
    }
}
