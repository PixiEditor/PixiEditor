using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
internal class MoveToolToolbar : Toolbar
{
    public bool KeepOriginalImage => GetSetting<BoolSetting>(nameof(KeepOriginalImage)).Value;

    public MoveToolToolbar()
    {
        Settings.Add(new BoolSetting(nameof(KeepOriginalImage), "Kеep original image"));
    }
}
