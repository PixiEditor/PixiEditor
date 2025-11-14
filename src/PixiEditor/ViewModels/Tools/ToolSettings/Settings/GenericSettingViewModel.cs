namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal class GenericSettingViewModel : Setting
{
    public GenericSettingViewModel(string name) : base(name)
    {
    }

    public override Type GetSettingType()
    {
        return typeof(object);
    }
}
